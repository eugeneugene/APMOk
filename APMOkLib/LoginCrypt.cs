using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace APMOkLib
{
    public static class LoginCrypt
    {
        private const char SplitChar = '\u0004';

        private static readonly Random random = new();

        private static readonly AesManaged AES = new()
        {
            KeySize = 256,
            Key = Encoding.ASCII.GetBytes("FE!c8OzsKIF\\>)o*1R{*$>w`O)DQZ@A<"),
            BlockSize = 128,
            IV = Encoding.ASCII.GetBytes("G4kkxPj!iq2LBe-L")
        };

        internal static byte[] Encrypt(byte[] _data)
        {
            var transform = AES.CreateEncryptor();
            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(_data, 0, _data.Length);
                cryptoStream.Flush();
            }

            memoryStream.Flush();
            return memoryStream.ToArray();
        }

        internal static byte[] Decrypt(byte[] _data)
        {
            int num;
            var transform = AES.CreateDecryptor();
            using var memoryStream = new MemoryStream(_data);
            using var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
            using var memoryStream2 = new MemoryStream();
            while ((num = cryptoStream.ReadByte()) > 0)
                memoryStream2.WriteByte((byte)num);
            memoryStream2.Flush();
            return memoryStream2.ToArray();
        }

        public static string EncryptLoginPwd(string login, string pwd, string salt)
        {
            return Convert.ToBase64String(Encrypt(Encoding.ASCII.GetBytes(login + SplitChar + pwd + SplitChar + salt)));
        }

        public static bool TryDecryptLoginPwd(string LoginPwd, out string login, out string pwd)
        {
            return TryDecryptLoginPwd(LoginPwd, out login, out pwd, out _);
        }

        public static bool TryDecryptLoginPwd(string LoginPwd, out string login, out string pwd, out string salt)
        {
            string dec = null;
            login = string.Empty;
            pwd = string.Empty;
            salt = string.Empty;

            if (string.IsNullOrEmpty(LoginPwd))
                return false;
            try
            {
                int bufsize = (LoginPwd.Length * 3 + 3) / 4 - (LoginPwd.Length > 0 && LoginPwd[^1] == '=' ? LoginPwd.Length > 1 && LoginPwd[^2] == '=' ? 2 : 1 : 0);
                byte[] buffer = new byte[bufsize];

                if (Convert.TryFromBase64String(LoginPwd, buffer, out int len))
                    dec = Encoding.ASCII.GetString(Decrypt(buffer));
            }
            catch
            {
                return false;
            }

            if (!string.IsNullOrEmpty(dec))
            {
                string[] array = dec.Split(SplitChar);

                if (array.Length > 0)
                {
                    if (array.Length > 0)
                        login = array[0];
                    if (array.Length > 1)
                        pwd = array[1];
                    if (array.Length > 2)
                        salt = array[2];
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidAuth(string LoginPwd)
        {
            return TryDecryptLoginPwd(LoginPwd, out _, out _);
        }

        public static string MakeSalt(int length)
        {
            const string chars = @" !""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
