namespace APMOkLib
{
    public class AuthParameter : JsonToString
    {
        public AuthParameter()
        {
            Login = Password = null;
            Valid = false;
        }

        public AuthParameter(string login, string password, bool valid)
        {
            Login = login;
            Password = password;
            Valid = valid;
        }

        public string Login { get; }
        public string Password { get; }
        public bool Valid { get; }
    }
}
