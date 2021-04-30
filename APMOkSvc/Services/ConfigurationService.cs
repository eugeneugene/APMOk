using APMData.Proto;
using APMOkSvc.Types;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    public class ConfigurationService : APMData.Proto.ConfigurationService.ConfigurationServiceBase
    {
        private readonly ILogger _logger;
        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            _logger = logger;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public override Task<DriveAPMConfigurationReply> GetDriveAPMConfiguration(Empty request, ServerCallContext context)
        {
            return base.GetDriveAPMConfiguration(request, context);
        }

        public override Task<Empty> SetDriveAPMConfiguration(DriveAPMConfigurationRequest request, ServerCallContext context)
        {
            return base.SetDriveAPMConfiguration(request, context);
        }
    }
}
