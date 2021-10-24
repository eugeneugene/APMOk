using Microsoft.Extensions.Configuration;
using System;

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
            if (authParameter is null)
                throw new ArgumentNullException(nameof(authParameter));

            _defaultValue = authParameter.Value ?? throw new ArgumentException("Value cannot be null");
        }

        public AuthParameter ExtractValue(IConfiguration configuration, string section)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrEmpty(section))
                throw new ArgumentException($"'{nameof(section)}' cannot be null or empty.", nameof(section));

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
