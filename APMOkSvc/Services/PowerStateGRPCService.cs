using APMData.Proto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    /// <summary>
    /// PowerState GRPC Service
    /// </summary>
    public class PowerStateGRPCService : PowerStateService.PowerStateServiceBase
    {
        private readonly ILogger _logger;
        private readonly PowerStatusContainer _powerStatusContainer;

        public PowerStateGRPCService(ILogger<PowerStateGRPCService> logger, PowerStatusContainer powerStatusContainer)
        {
            _logger = logger;
            _powerStatusContainer = powerStatusContainer;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public override Task<PowerStateReply> GetPowerState(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_powerStatusContainer.PowerState);
        }
    }
}
