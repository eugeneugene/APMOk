using APMData;
using APMOkLib;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Services
{
    class APM : IDisposable
    {
        private readonly GrpcChannel _channel;
        private bool disposedValue;

        public APM(ISocketPathProvider socketPathProvider)
        {
            var udsEndPoint = new UnixDomainSocketEndPoint(socketPathProvider.GetSocketPath());
            var connectionFactory = new UnixDomainSocketConnectionFactory(udsEndPoint);
            var socketsHttpHandler = new SocketsHttpHandler
            {
                ConnectCallback = connectionFactory.ConnectAsync,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                SslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = RemoteCertificateValidationCallback,
                }
            };

            _channel = GrpcChannel.ForAddress("https://localhost", new GrpcChannelOptions
            {
                HttpHandler = socketsHttpHandler,
            });
        }

        public async Task<CurrentAPMReply> GetCurrentAPMAsync(CurrentAPMRequest request, CancellationToken cancellationToken)
        {
            var client = new APMService.APMServiceClient(_channel);
            var reply = await client.GetCurrentAPMAsync(request, new CallOptions(cancellationToken: cancellationToken));
            return reply;
        }

        private static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
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
}
