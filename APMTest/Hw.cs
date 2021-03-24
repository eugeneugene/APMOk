using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace APMTest
{
    public class HW
    {
        [DllImport("hw.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int EnumerateDisks([In, Out] IntPtr diskInfo);

        [DllImport("hw.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetAPM([In] string dskName);

        public static int EnumerateDisks(out IEnumerable<EnumDiskInfo> diskInfo)
        {
            int EnumDiskInfoSize = Marshal.SizeOf(new EnumDiskInfo());
            IntPtr ptr = Marshal.AllocHGlobal(EnumDiskInfoSize * 16);
            try
            {
                var res = EnumerateDisks(ptr);
                if (res > 0)
                {
                    var result = new EnumDiskInfo[16];
                    for (int i = 0; i < 16; i++)
                        result[i] = (EnumDiskInfo)Marshal.PtrToStructure(new IntPtr(ptr.ToInt64() + (i * EnumDiskInfoSize)), typeof(EnumDiskInfo));
                    diskInfo = result;
                    return res;
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    Debug.WriteLine("Win32 Error: ", error);
                }

                diskInfo = null;
                return 0;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static int GetAPM(int Index)
        {
            string dskName = $"\\\\.\\PhysicalDrive{Index}";

            var result = GetAPM(dskName);
            if (result < 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return result;
        }
    }
}
