using Microsoft.Extensions.Configuration;

namespace APMOkLib.CustomConfiguration
{
    public class AuthParameterDecorator : IParameterDecorator<AuthParameter>
    {
        private readonly AuthParameter _defaultValue;

        public AuthParameter DefaultValue => _defaultValue;

        public AuthParameterDecorator()
        {
            _defaultValue = new();
        }

        public AuthParameterDecorator(IConfigurationParameter<AuthParameter> authParameter)
        {
            _defaultValue = authParameter.Value;
        }

        public AuthParameter ExtractValue(IConfiguration configuration, string section)
        {
            if (configuration.GetSection(section).Exists())
            {
                var value = configuration.GetValue(section, string.Empty);
                if (LoginCrypt.TryDecryptLoginPwd(value, out string login, out string password))
                    return new AuthParameter(login, password, true);
            }

            return _defaultValue;
        }
    }
}
