using APMData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    /// <summary>
    /// Configuration GRPC Service
    /// </summary>
    public class ConfigurationGRPCService : ConfigurationService.ConfigurationServiceBase
    {
        private readonly ILogger _logger;
        private readonly ConfigurationServiceImpl _configurationServiceImpl;

        public ConfigurationGRPCService(ILogger<ConfigurationGRPCService> logger, ConfigurationServiceImpl configurationServiceImpl)
        {
            _logger = logger;
            _configurationServiceImpl = configurationServiceImpl;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public override Task<DriveAPMConfigurationReply> GetDriveAPMConfiguration(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_configurationServiceImpl.GetDriveAPMConfiguration());
        }

        public override async Task<ResetDriveReply> ResetDriveAPMConfiguration(ResetDriveRequest request, ServerCallContext context)
        {
            return await _configurationServiceImpl.ResetDriveAPMConfigurationAsync(request, context.CancellationToken);
        }
    }
}
