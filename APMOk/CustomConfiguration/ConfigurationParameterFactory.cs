using Ardalis.SmartEnum;
using Microsoft.Extensions.Configuration;

namespace CustomConfiguration
{
    public static class ConfigurationParameterFactory
    {
        public static IConfigurationParameter<T> CreateParameter<T>(IConfiguration configuration, string name, string section, IParameterDecorator<T> defaultValueDecorator, string description)
            => new ConfigurationParameter<T>(configuration, name, section, defaultValueDecorator, description);

        private class ConfigurationParameter<T> : SmartEnum<ConfigurationParameter<T>, string>, IConfigurationParameter<T>
        {
            private readonly IConfiguration _configuration;
            private readonly string _section;
            private readonly IParameterDecorator<T> _defaultValueDecorator;
            private readonly string _description;

            public string Section => _section;
            public string Description => _description;

            public ConfigurationParameter(IConfiguration configuration, string name, string section, IParameterDecorator<T> defaultValueDecorator, string description)
                : base(name, name)
            {
                _configuration = configuration;
                _section = section;
                _defaultValueDecorator = defaultValueDecorator;
                _description = description;
            }

            T IConfigurationParameter<T>.Value => _defaultValueDecorator.ExtractValue(_configuration, _section);

            bool IConfigurationParameter<T>.Exists => _configuration.GetSection(_section).Exists();
        }
    }
}
