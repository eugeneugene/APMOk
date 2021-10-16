using APMData;
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
        private readonly PowerStateContainer _powerStatusContainer;

        public PowerStateGRPCService(ILogger<PowerStateGRPCService> logger, PowerStateContainer powerStatusContainer)
        {
            _logger = logger;
            _powerStatusContainer = powerStatusContainer;
            _logger.LogTrace("Creating {0}", GetType().Name);
        }

        public override Task<PowerStateReply> GetPowerState(Empty request, ServerCallContext context)
        {
            var reply = _powerStatusContainer.PowerState;

            _logger.LogTrace("Reply: {0}", reply);

            return Task.FromResult(reply);
        }
    }
}
