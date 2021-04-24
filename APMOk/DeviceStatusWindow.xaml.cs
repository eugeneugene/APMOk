using APMData.Proto;
using APMOkLib;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using APMOk.Services;

namespace APMOk
{
    /// <summary>
    /// Interaction logic for DeviceStatusWindow.xaml
    /// </summary>
    internal partial class DeviceStatusWindow : Window
    {
        private readonly APMOkData _data;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<DiskInfoEntry> DiskInfo { get; } = new();

        public ObservableCollection<KeyValuePair<string, object>> DiskInfoItems { get; } = new();

        public DeviceStatusWindowModel ViewModel { get; set; }

        public DeviceStatusWindow(IServiceProvider serviceProvider, APMOkData data)
        {
            InitializeComponent();

            _data = data;
            _serviceProvider = serviceProvider;

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
            LoadBatteryStatus();
        }

        private void LoadDisksInfo()
        {
            try
            {
                using var diskInfoService = _serviceProvider.GetRequiredService<Services.DiskInfoService>();

                var reply = diskInfoService.EnumerateDisksAsync().GetAwaiter().GetResult();
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

        private void LoadBatteryStatus()
        {
            try
            {
                using var diskInfoService = _serviceProvider.GetRequiredService<Services.PowerStateService>();
                var reply = diskInfoService.GetPowerStateAsync().GetAwaiter().GetResult();
                ViewModel.Battery = (ACLineStatus)reply.ACLineStatus;
            }
            catch (RpcException rex)
            {
                ViewModel.Battery = ACLineStatus.Error;
                Debug.WriteLine(rex.Message);
            }
        }

        private void SelectDiskComboSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            DiskInfoItems.Clear();
            if (e.OriginalSource is ComboBox comboBox && comboBox.SelectedItem is DiskInfoEntry Item)
            {
                var availability = Availability.FromValue((ushort)Item.Availability).Name;
                var apmValue = ((short)Item.APMValue < 0) ? "Unavailable" : Item.APMValue.ToString();
                DiskInfoItems.Add(new(nameof(Item.Availability), availability));
                DiskInfoItems.Add(new(nameof(Item.Caption), Item.Caption));
                DiskInfoItems.Add(new(nameof(Item.Description), Item.Description));
                DiskInfoItems.Add(new(nameof(Item.DeviceID), Item.DeviceID));
                DiskInfoItems.Add(new(nameof(Item.APMValue), apmValue));
                DiskInfoItems.Add(new(nameof(Item.Manufacturer), Item.Manufacturer));
                DiskInfoItems.Add(new(nameof(Item.Model), Item.Model));
                DiskInfoItems.Add(new(nameof(Item.Name), Item.Name));
                DiskInfoItems.Add(new(nameof(Item.SerialNumber), Item.SerialNumber));
                DiskInfoItems.Add(new(nameof(Item.Status), Item.Status));
            }
        }
    }
}
