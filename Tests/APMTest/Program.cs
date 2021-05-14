using APMData;
using APMData.Proto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace APMTest
{
    class Program
    {
        static void Main(string[] _)
        {
            try
            {
                using var channel = CreateChannel();
                var client = new DiskInfoService.DiskInfoServiceClient(channel);
                var reply = client.EnumerateDisks(new Empty());
                Console.WriteLine("Reply ResponseResult: " + reply.ReplyResult);
                foreach (DiskInfoEntry entry in reply.DiskInfoEntries)
                    Console.WriteLine("Reply DiskInfo entry: " + (entry.InfoValid ? entry.Caption : "<Invalid>"));
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.ToString());
                Console.WriteLine("Exception: {0}", ex);
            }
        }

        public static GrpcChannel CreateChannel()
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

            return GrpcChannel.ForAddress("https://localhost", new GrpcChannelOptions
            {
                HttpHandler = socketsHttpHandler,
            });
        }

        private static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
