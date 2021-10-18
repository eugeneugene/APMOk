using APMOkLib;
using Grpc.Net.Client;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace APMOk.Services
{
    internal class GrpcChannelProvider : IGrpcChannelProvider
    {
        private readonly ISocketPathProvider _socketPathProvider;

        public GrpcChannelProvider(ISocketPathProvider socketPathProvider)
        {
            _socketPathProvider = socketPathProvider;
        }

        private static bool TrueRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

        public GrpcChannel GetHttpGrpcChannel()
        {
            var udsEndPoint = new UnixDomainSocketEndPoint(_socketPathProvider.GetSocketPath());
            var connectionFactory = new UnixDomainSocketConnectionFactory(udsEndPoint);
            var socketsHttpHandler = new SocketsHttpHandler
            {
                ConnectCallback = connectionFactory.ConnectAsync,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };

            var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
            {
                HttpHandler = socketsHttpHandler,
            });

            return channel;
        }

        public GrpcChannel GetHttpsGrpcChannel(RemoteCertificateValidationCallback remoteCertificateValidationCallback = null)
        {
            var udsEndPoint = new UnixDomainSocketEndPoint(_socketPathProvider.GetSocketPath());
            var connectionFactory = new UnixDomainSocketConnectionFactory(udsEndPoint);
            var socketsHttpHandler = new SocketsHttpHandler
            {
                ConnectCallback = connectionFactory.ConnectAsync,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                SslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = remoteCertificateValidationCallback ?? TrueRemoteCertificateValidationCallback,
                }
            };

            var channel = GrpcChannel.ForAddress("https://localhost", new GrpcChannelOptions
            {
                HttpHandler = socketsHttpHandler,
            });

            return channel;
        }
    }
}
