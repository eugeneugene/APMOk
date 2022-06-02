using APMData;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Services;

/// <summary>
/// Управление питанием дисков
/// DI Lifetime: Singleton
/// </summary>
internal class APM : IDisposable
{
    private readonly GrpcChannel _channel;
    private bool disposedValue = false;

    public APM(IGrpcChannelProvider grpcChannelProvider)
    {
        if (grpcChannelProvider is null)
            throw new ArgumentNullException(nameof(grpcChannelProvider));

        _channel = grpcChannelProvider.GetHttpGrpcChannel();
    }

    public async Task<CurrentAPMReply> GetCurrentAPMAsync(CurrentAPMRequest request, CancellationToken cancellationToken)
    {
        var client = new APMService.APMServiceClient(_channel);
        var reply = await client.GetCurrentAPMAsync(request, new CallOptions(cancellationToken: cancellationToken));
        return reply;
    }

    public async Task<APMReply> SetAPMAsync(APMRequest request, CancellationToken cancellationToken)
    {
        var client = new APMService.APMServiceClient(_channel);
        var reply = await client.SetAPMAsync(request, new CallOptions(cancellationToken: cancellationToken));
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
