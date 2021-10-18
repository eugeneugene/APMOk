using APMData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Services
{
    /// <summary>
    /// Получить информацию о текущем состоянии питания
    /// DI Lifetime: Transient
    /// </summary>
    internal class PowerState 
    {
        private readonly IGrpcChannelProvider _grpcChannelProvider;

        public PowerState(IGrpcChannelProvider grpcChannelProvider)
        {
            _grpcChannelProvider = grpcChannelProvider;
        }

        public async Task<PowerStateReply> GetPowerStateAsync(CancellationToken cancellationToken)
        {
            using var channel = _grpcChannelProvider.GetHttpGrpcChannel();
            var client = new PowerStateService.PowerStateServiceClient(channel);
            var reply = await client.GetPowerStateAsync(new Empty(), new CallOptions(cancellationToken: cancellationToken));
            return reply;
        }
    }
}
