using APMData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Services
{
    /// <summary>
    /// Получить информацию о текущих настройках APM
    /// DI Lifetime: Transient
    /// </summary>
    internal class Configuration
    {
        private readonly IGrpcChannelProvider _grpcChannelProvider;

        public Configuration(IGrpcChannelProvider grpcChannelProvider)
        {
            _grpcChannelProvider = grpcChannelProvider;
        }

        public async Task<DriveAPMConfigurationReply> GetDriveAPMConfigurationAsync(CancellationToken cancellationToken)
        {
            using var channel = _grpcChannelProvider.GetHttpGrpcChannel();
            var client = new ConfigurationService.ConfigurationServiceClient(channel);
            var reply = await client.GetDriveAPMConfigurationAsync(new Empty(), new CallOptions(cancellationToken: cancellationToken));
            return reply;
        }

        public async Task<ResetDriveReply> ResetDriveAPMConfigurationAsync(ResetDriveRequest request, CancellationToken cancellationToken)
        {
            using var channel = _grpcChannelProvider.GetHttpGrpcChannel();
            var client = new ConfigurationService.ConfigurationServiceClient(channel);
            var reply = await client.ResetDriveAPMConfigurationAsync(request, new CallOptions(cancellationToken: cancellationToken));
            return reply;
        }
    }
}
