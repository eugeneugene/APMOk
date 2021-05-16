using APMData;
using APMData.Proto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace APMOk.Services
{
    /// <summary>
    /// Получить информацию о системных дисках
    /// DI Lifetime: Transient
    /// </summary>
    public class DiskInfoService : IDisposable
    {
        private readonly GrpcChannel _channel;
        private bool disposedValue;

        public DiskInfoService()
        {
            var udsEndPoint = new UnixDomainSocketEndPoint(SocketData.SocketPath);
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

        public async Task<SystemDiskInfoReply> EnumerateDisksAsync()
        {
            var client = new APMData.Proto.DiskInfoService.DiskInfoServiceClient(_channel);
            var reply = await client.EnumerateDisksAsync(new Empty());
            return reply;
        }

        public async Task<GetAPMReply> GetAPMAsync(GetAPMRequest request)
        {
            var client = new APMData.Proto.DiskInfoService.DiskInfoServiceClient(_channel);
            var reply = await client.GetAPMAsync(request);
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
