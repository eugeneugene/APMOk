using APMData;
using APMData.Code;
using APMData.Proto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Data;

namespace APMOk
{
    /// <summary>
    /// Interaction logic for DeviceStatusWindow.xaml
    /// </summary>
    public partial class DeviceStatusWindow : Window
    {
        private readonly ObservableCollection<DiskInfoEntry> _diskInfo = new() ;

        public DeviceStatusWindow()
        {
            InitializeComponent();

            CollectionViewSource driveStatusViewSource;
            driveStatusViewSource = (CollectionViewSource)FindResource("DriveStatusViewSource");
            driveStatusViewSource.Source = _diskInfo;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadDisksInfo();
        }

        private void LoadDisksInfo()
        {
            try
            {
                using var channel = CreateChannel();
                var client = new DiskInfoService.DiskInfoServiceClient(channel);
                var reply = client.EnumerateDisks(new Empty());
                _diskInfo.Clear();
                if (reply.ResponseResult == 0)
                {
                    foreach (var entry in reply.DiskInfoEntries.Where(item => item.InfoValid))
                        _diskInfo.Add(entry);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
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

        private static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
