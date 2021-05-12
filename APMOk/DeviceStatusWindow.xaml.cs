using APMData;
using APMData.Proto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace APMOk
{
    /// <summary>
    /// Interaction logic for DeviceStatusWindow.xaml
    /// </summary>
    internal partial class DeviceStatusWindow : Window, IDisposable
    {
        private readonly APMOkData _apmOkData;
        private readonly Services.ConfigurationService _configurationService;
        private bool disposedValue;

        public ObservableCollection<DiskInfoEntry> DiskInfo { get; } = new();

        public ObservableCollection<KeyValuePair<string, object>> DiskInfoItems { get; } = new();

        public DeviceStatusWindow(APMOkData apmOkData, Services.ConfigurationService configurationService)
        {
            InitializeComponent();

            _apmOkData = apmOkData;
            _configurationService = configurationService;

            // DeviceStatusDataSource
            CollectionViewSource deviceStatusDataSource = FindResource("DeviceStatusDataSource") as CollectionViewSource;
            deviceStatusDataSource.Source = DiskInfo;

            // DiskInfoItemsSource
            CollectionViewSource diskInfoItemsSource = FindResource("DiskInfoItemsSource") as CollectionViewSource;
            diskInfoItemsSource.Source = DiskInfoItems;

            DataContext = _apmOkData;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadDisksInfo();
            _apmOkData.PropertyChanged += APMOkDataPropertyChanged;
        }

        private void APMOkDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SystemDiskInfo":
                    LoadDisksInfo();
                    break;
                case "PowerState":
                    break;
                case "ConnectFailure":
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void LoadDisksInfo()
        {
            if (_apmOkData.SystemDiskInfo == null || _apmOkData.SystemDiskInfo.ReplyResult == 0)
                return;

            Dispatcher.Invoke(() =>
            {
                if (DiskInfo.Any())
                    return;

                foreach (var entry in _apmOkData.SystemDiskInfo.DiskInfoEntries.Where(item => item.InfoValid).OrderBy(item => item.DeviceID))
                    DiskInfo.Add(entry);

                if (DiskInfo.Any())
                    SelectDiskCombo.SelectedIndex = 0;
                else
                    SelectDiskCombo.SelectedIndex = -1;
            });
        }

        private void SelectDiskComboSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

            Dispatcher.Invoke(async () =>
            {
                DiskInfoItems.Clear();
                if (e.OriginalSource is ComboBox comboBox && comboBox.SelectedItem is DiskInfoEntry Item)
                {
                    var availability = Availability.FromValue((ushort)Item.Availability).Name;
                    DiskInfoItems.Add(new(nameof(Item.Availability), availability));
                    DiskInfoItems.Add(new(nameof(Item.Caption), Item.Caption));
                    DiskInfoItems.Add(new(nameof(Item.Description), Item.Description));
                    DiskInfoItems.Add(new(nameof(Item.DeviceID), Item.DeviceID));
                    DiskInfoItems.Add(new(nameof(Item.Manufacturer), Item.Manufacturer));
                    DiskInfoItems.Add(new(nameof(Item.Model), Item.Model));
                    DiskInfoItems.Add(new(nameof(Item.SerialNumber), Item.SerialNumber));
                }

                var configReply = await _configurationService.GetDriveAPMConfiguration();
                if (configReply!= null && configReply.ReplyResult!=0)
                {
                    _apmOkData.APMValueProperty = new(configReply.DriveAPMConfigurationReplyEntries.)
                }
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _apmOkData.PropertyChanged -= APMOkDataPropertyChanged;
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

        private void SelectDiskComboDropDownOpened(object sender, EventArgs e)
        {
            if (_apmOkData.SystemDiskInfo == null || _apmOkData.SystemDiskInfo.ReplyResult == 0)
                return;

            Dispatcher.Invoke(() =>
            {
                string selectedSerial = null;
                if (SelectDiskCombo.SelectedItem is DiskInfoEntry selectedItem)
                    selectedSerial = selectedItem.SerialNumber;

                // Items to delete from combobox
                var deleteItems = DiskInfo.Where(item => !_apmOkData.SystemDiskInfo.DiskInfoEntries.Where(item => item.InfoValid).Any(item1 => item1.Equals(item)));
                foreach (var item in deleteItems)
                    DiskInfo.Remove(item);

                // Items to add to combobox
                var addItems = _apmOkData.SystemDiskInfo.DiskInfoEntries.Where(item => item.InfoValid && !DiskInfo.Any(item1 => item1.Equals(item)));
                foreach (var item in addItems)
                    DiskInfo.Add(item);

                List<DiskInfoEntry> DiskInfoTmp = new();
                DiskInfoTmp.AddRange(DiskInfo);

                DiskInfo.Clear();
                foreach (var item in DiskInfoTmp.OrderBy(item => item.DeviceID))
                    DiskInfo.Add(item);

                var selectItem = DiskInfo.SingleOrDefault(item => item.SerialNumber == selectedSerial);
                if (selectItem != null)
                    SelectDiskCombo.SelectedItem = selectItem;
                else
                {
                    if (_apmOkData.SystemDiskInfo.DiskInfoEntries.Any())
                        SelectDiskCombo.SelectedIndex = 0;
                    else
                        SelectDiskCombo.SelectedIndex = -1;
                }
            });
        }
    }
}
