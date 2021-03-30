using APMData;
using APMData.Code;
using APMData.Proto;
using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Net.Sockets;

namespace APMTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using var channel = CreateChannel();
                var client = new APMData.Proto.APMData.APMDataClient(channel);
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
                Console.WriteLine("Exception: {0}", ex.Message);
            }
        }

        public static GrpcChannel CreateChannel()
        {
            var udsEndPoint = new UnixDomainSocketEndPoint(SocketData.SocketPath);
            var connectionFactory = new UnixDomainSocketConnectionFactory(udsEndPoint);
            var socketsHttpHandler = new SocketsHttpHandler
            {
                ConnectCallback = connectionFactory.ConnectAsync
            };

            return GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
            {
                HttpHandler = socketsHttpHandler
            });
        }
    }
}
