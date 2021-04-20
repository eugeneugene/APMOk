using APMData;
using APMData.Code;
using APMData.Proto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace APMOk
{
    /// <summary>
    /// Interaction logic for DeviceStatusWindow.xaml
    /// </summary>
    internal partial class DeviceStatusWindow : Window
    {
        public ObservableCollection<DiskInfoEntry> DiskInfo { get; } = new();

        public ObservableCollection<KeyValuePair<string, object>> DiskInfoItems { get; } = new();

        public DeviceStatusWindowModel ViewModel { get; set; }

        public DeviceStatusWindow()
        {
            InitializeComponent();

            // DeviceStatusDataSource
            CollectionViewSource deviceStatusDataSource = FindResource("DeviceStatusDataSource") as CollectionViewSource;
            deviceStatusDataSource.Source = DiskInfo;

            // DiskInfoItemsSource
            CollectionViewSource diskInfoItemsSource = FindResource("DiskInfoItemsSource") as CollectionViewSource;
            diskInfoItemsSource.Source = DiskInfoItems;

            DataContext = ViewModel = new DeviceStatusWindowModel();
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
                    ViewModel.Connected = true;
                }
            }
            catch (RpcException rex)
            {
                ViewModel.Connected = false;
                Debug.WriteLine(rex.Message);
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

        private void SelectDiskComboSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            DiskInfoItems.Clear();
            if (e.OriginalSource is ComboBox comboBox && comboBox.SelectedItem is DiskInfoEntry Item)
            {
                DiskInfoItems.Add(new(nameof(Item.Availability), Item.Availability));
                DiskInfoItems.Add(new(nameof(Item.Caption), Item.Caption));
                DiskInfoItems.Add(new(nameof(Item.Description), Item.Description));
                DiskInfoItems.Add(new(nameof(Item.DeviceID), Item.DeviceID));
                DiskInfoItems.Add(new(nameof(Item.APMValue), Item.APMValue));
                DiskInfoItems.Add(new(nameof(Item.Manufacturer), Item.Manufacturer));
                DiskInfoItems.Add(new(nameof(Item.Model), Item.Model));
                DiskInfoItems.Add(new(nameof(Item.Name), Item.Name));
                DiskInfoItems.Add(new(nameof(Item.SerialNumber), Item.SerialNumber));
                DiskInfoItems.Add(new(nameof(Item.Status), Item.Status));
            }
        }
    }
}
