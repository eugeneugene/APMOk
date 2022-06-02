using APMOkLib.CustomConfiguration;
using Microsoft.Extensions.Configuration;

namespace APMOkSvc.Code;

public class ConnectionStringsConfiguration : SmartConfiguration, IConnectionStringsConfiguration
{
    public IConfigurationParameter<string> DataContext { get; }

    public ConnectionStringsConfiguration(IConfiguration configuration, ConfigurationParameterFactory parameterFactory)
        : base("ConnectionStrings", configuration)
    {
        DataContext = parameterFactory.CreateParameter(this, nameof(DataContext), nameof(DataContext), new SimpleParameterDecorator<string>(string.Empty),
            "DataContext connection string");
    }
}
