using APMData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Services;

/// <summary>
/// Получить информацию о версии сервиса
/// DI Lifetime: Singleton
/// </summary>
internal class Version : IDisposable
{
    private readonly GrpcChannel _channel;
    private bool disposedValue;

    public Version(IGrpcChannelProvider grpcChannelProvider)
    {
        if (grpcChannelProvider is null)
            throw new ArgumentNullException(nameof(grpcChannelProvider));

        _channel = grpcChannelProvider.GetHttpGrpcChannel();
    }

    public async Task<ServiceVersionReply> GetVersionAsync(CancellationToken cancellationToken)
    {
        var client = new VersionService.VersionServiceClient(_channel);
        var reply = await client.GetServiceVersionAsync(new Empty(), new CallOptions(cancellationToken: cancellationToken));
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
