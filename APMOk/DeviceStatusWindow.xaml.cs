using APMData.Proto;
using APMOk.Code;
using APMOkLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly Services.DiskInfoService _diskInfoService;
        private bool disposedValue;

        public ObservableCollection<DiskInfoEntry> DiskInfo { get; } = new();

        public ObservableConcurrentDictionary<string, object> DiskInfoItems { get; } = new();

        public ObservableConcurrentDictionary<string, object> PowerStateItems { get; } = new();

        public APMValueProperty APMValue { get; }

        public DeviceStatusWindow(APMOkData apmOkData, Services.DiskInfoService diskInfoService)
        {
            InitializeComponent();

            _apmOkData = apmOkData;
            _diskInfoService = diskInfoService;

            // DeviceStatusDataSource
            CollectionViewSource deviceStatusDataSource = FindResource("DeviceStatusDataSource") as CollectionViewSource;
            deviceStatusDataSource.Source = DiskInfo;

            DriveStatusGrid.DataContext = DiskInfoItems;
            BatteryStatusGrid.DataContext = PowerStateItems;

            DataContext = _apmOkData;

            APMValue = FindResource("APMValue") as APMValueProperty;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadDisksInfo();
            LoadPowerState();
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
                    LoadPowerState();
                    break;
                case "ConnectFailure":
                    break;
                case "APMValueDictionary":
                    LoadAPMValue();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void LoadPowerState()
        {
            if (_apmOkData.PowerState != null && _apmOkData.PowerState.ReplyResult != 0)
            {
                PowerStateItems[nameof(_apmOkData.PowerState.ACLineStatus)] = _apmOkData.PowerState.BatteryFlag;
                PowerStateItems[nameof(_apmOkData.PowerState.BatteryFlag)] = _apmOkData.PowerState.ACLineStatus;
                PowerStateItems[nameof(_apmOkData.PowerState.BatteryFullLifeTime)] = _apmOkData.PowerState.BatteryFullLifeTime;
                PowerStateItems[nameof(_apmOkData.PowerState.BatteryLifePercent)] = _apmOkData.PowerState.BatteryLifePercent;
                PowerStateItems[nameof(_apmOkData.PowerState.BatteryLifeTime)] = _apmOkData.PowerState.BatteryLifeTime;
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

            if (e.OriginalSource is ComboBox comboBox && comboBox.SelectedItem is DiskInfoEntry selectedItem)
            {
                var availability = Availability.FromValue((ushort)selectedItem.Availability).Name;
                DiskInfoItems[nameof(selectedItem.Availability)] = availability;
                DiskInfoItems[nameof(selectedItem.Caption)] = selectedItem.Caption;
                DiskInfoItems[nameof(selectedItem.Description)] = selectedItem.Description;
                DiskInfoItems[nameof(selectedItem.DeviceID)] = selectedItem.DeviceID;
                DiskInfoItems[nameof(selectedItem.Manufacturer)] = selectedItem.Manufacturer;
                DiskInfoItems[nameof(selectedItem.Model)] = selectedItem.Model;
                DiskInfoItems[nameof(selectedItem.SerialNumber)] = selectedItem.SerialNumber;
                LoadAPMValue();
            }
        }

        private void LoadAPMValue()
        {
            if (SelectDiskCombo.SelectedItem is DiskInfoEntry selectedItem)
            {
                if (_apmOkData.APMValueDictionary.Any(item => item.Key == selectedItem.DeviceID))
                {
                    var device = _apmOkData.APMValueDictionary.Single(item => item.Key == selectedItem.DeviceID);
                    APMValue.UserValue = device.Value.UserValue;
                    APMValue.DefaultValue = device.Value.DefaultValue;

                    Task.Run(async () =>
                    {
                        var apmReply = await _diskInfoService.GetAPMAsync(new GetAPMRequest { DeviceID = device.Key });
                        APMValue.CurrentValue = apmReply.ReplyResult != 0 ? (int)apmReply.APMValue : -1;
                    });
                }
            }
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
        }
    }
}
