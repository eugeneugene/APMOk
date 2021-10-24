using Microsoft.Extensions.Configuration;
using System;

namespace APMOkLib.CustomConfiguration
{
    public class SimpleParameterDecorator<T> : IParameterDecorator<T>
    {
        private readonly T? _defaultValue;

        public T? DefaultValue => _defaultValue;

        public SimpleParameterDecorator(T defaultValue)
        {
            _defaultValue = defaultValue ?? throw new ArgumentException("Default Value cannot be null");
        }

        public SimpleParameterDecorator(IConfigurationParameter<T>? defaultValue)
        {
            if (defaultValue is null)
                throw new ArgumentNullException(nameof(defaultValue));
            
            _defaultValue = defaultValue.Value;
        }

        public T ExtractValue(IConfiguration configuration, string section)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrEmpty(section))
                throw new ArgumentException($"'{nameof(section)}' cannot be null or empty.", nameof(section));

            return configuration.GetValue(section, _defaultValue)!;
        }
    }
}
