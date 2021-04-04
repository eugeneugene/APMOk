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
    internal partial class DeviceStatusWindow : Window
    {
        public ObservableCollection<DiskInfoEntry> DiskInfo { get; } = new();

        public DeviceStatusWindow()
        {
            InitializeComponent();

            //DeviceStatusDataSource
            CollectionViewSource itemCollectionViewSource;
            itemCollectionViewSource = (CollectionViewSource)FindResource("DeviceStatusDataSource");
            itemCollectionViewSource.Source = DiskInfo;
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
                DiskInfo.Clear();
                if (reply.ResponseResult == 0)
                {
                    foreach (var entry in reply.DiskInfoEntries.Where(item => item.InfoValid))
                        DiskInfo.Add(entry);

                    if (DiskInfo.Any())
                        SelectDiskCombo.SelectedIndex = 0;
                    else
                        SelectDiskCombo.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
