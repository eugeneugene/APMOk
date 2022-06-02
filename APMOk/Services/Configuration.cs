using APMData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Services;

/// <summary>
/// Получить информацию о текущих настройках APM
/// DI Lifetime: Singleton
/// </summary>
internal class Configuration : IDisposable
{
    private readonly GrpcChannel _channel;
    private bool disposedValue;

    public Configuration(IGrpcChannelProvider grpcChannelProvider)
    {
        if (grpcChannelProvider is null)
            throw new ArgumentNullException(nameof(grpcChannelProvider));

        _channel = grpcChannelProvider.GetHttpGrpcChannel();
    }

    public async Task<DriveAPMConfigurationReply> GetDriveAPMConfigurationAsync(CancellationToken cancellationToken)
    {
        var client = new ConfigurationService.ConfigurationServiceClient(_channel);
        var reply = await client.GetDriveAPMConfigurationAsync(new Empty(), new CallOptions(cancellationToken: cancellationToken));
        return reply;
    }

    public async Task<ResetDriveReply> ResetDriveAPMConfigurationAsync(ResetDriveRequest request, CancellationToken cancellationToken)
    {
        var client = new ConfigurationService.ConfigurationServiceClient(_channel);
        var reply = await client.ResetDriveAPMConfigurationAsync(request, new CallOptions(cancellationToken: cancellationToken));
        return reply;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _channel.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
