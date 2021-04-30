using Microsoft.Extensions.Configuration;

namespace CustomConfiguration
{
    public class SimpleParameterDecorator<T> : IParameterDecorator<T>
    {
        private readonly T _defaultValue;

        public SimpleParameterDecorator(T defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public T ExtractValue(IConfiguration configuration, string section) => configuration .GetValue(section, _defaultValue);
    }
}
