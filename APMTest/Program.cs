using APMData;
using APMData.Code;
using APMData.Proto;
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
        static void Main(string[] args)
        {
            try
            {
                using var channel = CreateChannel();
                var client = new DiskInfoService.DiskInfoServiceClient(channel);
                var reply = client.EnumerateDisks(new Empty());
                Console.WriteLine("Reply ResponseResult: " + reply.ResponseResult);
                Console.WriteLine("Reply ResponseTimeStamp: " + reply.ResponseTimeStamp);
                foreach (var entry in reply.DiskInfoEntries)
                    Console.WriteLine("Reply DiskInfo entry: " + entry.Caption);
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

            //var handler = new HttpClientHandler
            //{
            //    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            //    ClientCertificateOptions = ClientCertificateOption.Manual,
            //    ServerCertificateCustomValidationCallback = ServerCertificateCustomValidationCallback,
            //};

            return GrpcChannel.ForAddress("https://localhost", new GrpcChannelOptions
            {
                HttpHandler = socketsHttpHandler,
                //HttpClient = new HttpClient(handler, disposeHandler: true),
            });
        }

        private static bool ServerCertificateCustomValidationCallback(HttpRequestMessage httpRequestMessage, X509Certificate2 x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
        {
            if (x509Certificate == null)
                throw new ArgumentNullException(nameof(x509Certificate));

            Console.WriteLine("HttpRequestMessage: {0} Server Certificate: Subject: {1} Issuer: {2} NotBefore: {3} NotAfter: {4} Thumbprint: {5}",
                httpRequestMessage, x509Certificate.Subject, x509Certificate.Issuer, x509Certificate.NotBefore, x509Certificate.NotAfter, x509Certificate.Thumbprint);

            if (!x509Certificate.Verify())
                Console.WriteLine("Сертификат невалидный!");

            return true;
        }

        private static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
