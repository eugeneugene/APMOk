using APMData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Services;

/// <summary>
/// Получить информацию о системных дисках
/// DI Lifetime: Singleton
/// </summary>
internal class DiskInfo : IDisposable
{
    private readonly GrpcChannel _channel;
    private bool disposedValue;

    public DiskInfo(IGrpcChannelProvider grpcChannelProvider)
    {
        if (grpcChannelProvider is null)
            throw new ArgumentNullException(nameof(grpcChannelProvider));

        _channel = grpcChannelProvider.GetHttpGrpcChannel();
    }

    public async Task<DisksReply> EnumerateDisksAsync(CancellationToken cancellationToken)
    {
        var client = new DiskInfoService.DiskInfoServiceClient(_channel);
        var reply = await client.EnumerateDisksAsync(new Empty(), new CallOptions(cancellationToken: cancellationToken));
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
