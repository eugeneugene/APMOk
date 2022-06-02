using APMOkLib;
using Grpc.Net.Client;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace APMOk.Services;

/// <summary>
/// GrpcChannelProvider
/// DI Lifetime: Singleton
/// </summary>
internal class GrpcChannelProvider : IGrpcChannelProvider
{
    private readonly UnixDomainSocketConnectionFactory _connectionFactory;

    public GrpcChannelProvider(ISocketPathProvider socketPathProvider)
    {
        if (socketPathProvider is null)
            throw new ArgumentNullException(nameof(socketPathProvider));

        var _udsEndPoint = new UnixDomainSocketEndPoint(socketPathProvider.GetSocketPath());
        _connectionFactory = new UnixDomainSocketConnectionFactory(_udsEndPoint);
    }

    private static bool TrueRemoteCertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true;

    public GrpcChannel GetHttpGrpcChannel()
    {
        var socketsHttpHandler = new SocketsHttpHandler
        {
            ConnectCallback = _connectionFactory.ConnectAsync,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };

        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpHandler = socketsHttpHandler,
        });

        return channel;
    }

    public GrpcChannel GetHttpsGrpcChannel(RemoteCertificateValidationCallback? remoteCertificateValidationCallback = null)
    {
        var socketsHttpsHandler = new SocketsHttpHandler
        {
            ConnectCallback = _connectionFactory.ConnectAsync,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = remoteCertificateValidationCallback ?? TrueRemoteCertificateValidationCallback,
            }
        };

        var channel = GrpcChannel.ForAddress("https://localhost", new GrpcChannelOptions
        {
            HttpHandler = socketsHttpsHandler,
        });

        return channel;
    }
}
