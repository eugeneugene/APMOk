using CustomConfiguration;
using Microsoft.Extensions.Configuration;

namespace APMOkSvc.Code
{
    public class ConnectionStringsConfiguration : ICustomConfiguration
    {
        public IConfiguration Configuration { get; }
        public string SectionName => "ConnectionStrings";

        public IConfigurationParameter<string> DataContext { get; }
        public IConfigurationParameter<string> EntervoContext { get; }

        public ConnectionStringsConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
            DataContext = ConfigurationParameterFactory.CreateParameter(Configuration, nameof(DataContext), SectionName + ":" + nameof(DataContext), new SimpleParameterDecorator<string>(string.Empty),
                "Строка подключения DataContext (по-умолчанию пустая строка)");
            EntervoContext = ConfigurationParameterFactory.CreateParameter(Configuration, nameof(EntervoContext), SectionName + ":" + nameof(EntervoContext), new SimpleParameterDecorator<string>(string.Empty),
                "Строка подключения EntervoContext (по-умолчанию пустая строка)");
        }
    }
}
