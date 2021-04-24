using Microsoft.Extensions.Configuration;

namespace CustomConfiguration
{
    public interface IParameterDecorator<T>
    {
        T ExtractValue(IConfiguration configuration, string section);
    }
}
