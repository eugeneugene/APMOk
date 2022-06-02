using APMData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Services;

/// <summary>
/// Получить информацию о текущем состоянии питания
/// DI Lifetime: Singleton
/// </summary>
internal class PowerState : IDisposable
{
    private readonly GrpcChannel _channel;
    private bool disposedValue;

    public PowerState(IGrpcChannelProvider grpcChannelProvider)
    {
        if (grpcChannelProvider is null)
            throw new ArgumentNullException(nameof(grpcChannelProvider));

        _channel = grpcChannelProvider.GetHttpGrpcChannel();
    }

    public async Task<PowerStateReply> GetPowerStateAsync(CancellationToken cancellationToken)
    {
        var client = new PowerStateService.PowerStateServiceClient(_channel);
        var reply = await client.GetPowerStateAsync(new Empty(), new CallOptions(cancellationToken: cancellationToken));
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
