using APMData.Proto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    public class PowerStateService : APMData.Proto.PowerStateService.PowerStateServiceBase
    {
        private readonly ILogger _logger;
        private readonly PowerStateServiceImpl _powerStateServiceImpl;

        public PowerStateService(ILogger<PowerStateService> logger, PowerStateServiceImpl powerStateServiceImpl)
        {
            _logger = logger;
            _powerStateServiceImpl = powerStateServiceImpl;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public override Task<PowerStateReply> GetPowerState(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_powerStateServiceImpl.GetPowerState());
        }
    }
}
