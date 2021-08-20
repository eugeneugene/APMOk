using Microsoft.Extensions.Configuration;

namespace APMOkLib.CustomConfiguration
{
    public interface IParameterDecorator<T>
    {
        T ExtractValue(IConfiguration configuration, string section);
        T DefaultValue { get; }
    }
}
