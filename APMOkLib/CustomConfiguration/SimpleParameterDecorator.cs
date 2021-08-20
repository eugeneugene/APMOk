using Microsoft.Extensions.Configuration;

namespace APMOkLib.CustomConfiguration
{
    public class SimpleParameterDecorator<T> : IParameterDecorator<T>
    {
        private readonly T _defaultValue;

        public T DefaultValue => _defaultValue;

        public SimpleParameterDecorator(T defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public SimpleParameterDecorator(IConfigurationParameter<T> defaultValue)
        {
            _defaultValue = defaultValue.Value;
        }

        public T ExtractValue(IConfiguration configuration, string section) => configuration.GetValue(section, _defaultValue);
    }
}
