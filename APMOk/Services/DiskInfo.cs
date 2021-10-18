using APMData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Services
{
    /// <summary>
    /// Получить информацию о системных дисках
    /// DI Lifetime: Transient
    /// </summary>
    internal class DiskInfo
    {
        private readonly IGrpcChannelProvider _grpcChannelProvider;

        public DiskInfo(IGrpcChannelProvider grpcChannelProvider)
        {
            _grpcChannelProvider = grpcChannelProvider;
        }

        public async Task<DisksReply> EnumerateDisksAsync(CancellationToken cancellationToken)
        {
            using var channel = _grpcChannelProvider.GetHttpGrpcChannel();
            var client = new DiskInfoService.DiskInfoServiceClient(channel);
            var reply = await client.EnumerateDisksAsync(new Empty(), new CallOptions(cancellationToken: cancellationToken));
            return reply;
        }
    }
}
