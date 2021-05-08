using APMData.Proto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    public class ConfigurationService : APMData.Proto.ConfigurationService.ConfigurationServiceBase
    {
        private readonly ILogger _logger;
        private readonly ConfigurationServiceImpl _configurationServiceImpl;

        public ConfigurationService(ILogger<ConfigurationService> logger, ConfigurationServiceImpl configurationServiceImpl)
        {
            _logger = logger;
            _configurationServiceImpl = configurationServiceImpl;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public override Task<DriveAPMConfigurationReply> GetDriveAPMConfiguration(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_configurationServiceImpl.GetDriveAPMConfiguration());
        }
    }
}
