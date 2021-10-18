using APMData;
using Grpc.Core;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Services
{
    /// <summary>
    /// Управление питанием дисков
    /// DI Lifetime: Transient
    /// </summary>
    internal class APM 
    {
        private readonly IGrpcChannelProvider _grpcChannelProvider;

        public APM(IGrpcChannelProvider grpcChannelProvider)
        {
            _grpcChannelProvider = grpcChannelProvider;
        }

        public async Task<CurrentAPMReply> GetCurrentAPMAsync(CurrentAPMRequest request, CancellationToken cancellationToken)
        {
            using var channel = _grpcChannelProvider.GetHttpGrpcChannel();
            var client = new APMService.APMServiceClient(channel);
            var reply = await client.GetCurrentAPMAsync(request, new CallOptions(cancellationToken: cancellationToken));
            return reply;
        }

        public async Task<APMReply> SetAPMAsync(APMRequest request, CancellationToken cancellationToken)
        {
            using var channel = _grpcChannelProvider.GetHttpGrpcChannel();
            var client = new APMService.APMServiceClient(channel);
            var reply = await client.SetAPMAsync(request, new CallOptions(cancellationToken: cancellationToken));
            return reply;
        }
    }
}
