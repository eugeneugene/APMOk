using APMData;
using APMData.Proto;
using APMOk.Code;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        public ObservableConcurrentDictionary<string, object> DiskInfoItems { get; } = new();

        public ObservableConcurrentDictionary<string, object> PowerStateItems { get; } = new();

        public APMValueProperty APMValue { get; }

        public DeviceStatusWindow(APMOkData apmOkData, Services.ConfigurationService configurationService)
        {
            InitializeComponent();

            _apmOkData = apmOkData;
            _configurationService = configurationService;

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
                    SetAPMValue();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void LoadPowerState()
        {
            if (_apmOkData.PowerState.ReplyResult != 0)
            {
                PowerStateItems[nameof(_apmOkData.PowerState.ACLineStatus)] = _apmOkData.PowerState.BatteryFlag;
                PowerStateItems[nameof(_apmOkData.PowerState.BatteryFlag)] = _apmOkData.PowerState.ACLineStatus;
                PowerStateItems[nameof(_apmOkData.PowerState.BatteryFullLifeTime)] = _apmOkData.PowerState.BatteryFullLifeTime;
                PowerStateItems[nameof(_apmOkData.PowerState.BatteryLifePercent)] = _apmOkData.PowerState.BatteryLifePercent;
                PowerStateItems[nameof(_apmOkData.PowerState.BatteryLifeTime)] = _apmOkData.PowerState.BatteryLifeTime;
            }
        }

        private void SetAPMValue()
        {
            if (SelectDiskCombo.SelectedItem is DiskInfoEntry selectedItem)
            {
                if (_apmOkData.APMValueDictionary.Any(item => item.Key == selectedItem.DeviceID))
                {
                    var device = _apmOkData.APMValueDictionary.Single(item => item.Key == selectedItem.DeviceID);
                    APMValue.CurrentValue = device.Value.CurrentValue;
                    APMValue.DefaultValue = device.Value.DefaultValue;
                }
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

            Dispatcher.InvokeAsync(async () =>
            {
                if (e.OriginalSource is ComboBox comboBox && comboBox.SelectedItem is DiskInfoEntry Item)
                {
                    var availability = Availability.FromValue((ushort)Item.Availability).Name;
                    DiskInfoItems[nameof(Item.Availability)] = availability;
                    DiskInfoItems[nameof(Item.Caption)] = Item.Caption;
                    DiskInfoItems[nameof(Item.Description)] = Item.Description;
                    DiskInfoItems[nameof(Item.DeviceID)] = Item.DeviceID;
                    DiskInfoItems[nameof(Item.Manufacturer)] = Item.Manufacturer;
                    DiskInfoItems[nameof(Item.Model)] = Item.Model;
                    DiskInfoItems[nameof(Item.SerialNumber)] = Item.SerialNumber;
                }

                try
                {
                    var APMConfigurationReply = await _configurationService.GetDriveAPMConfigurationAsync();
                    if (APMConfigurationReply != null && APMConfigurationReply.ReplyResult != 0)
                    {
                        foreach (var entry in APMConfigurationReply.DriveAPMConfigurationReplyEntries)
                            _apmOkData.APMValueDictionary[entry.DeviceID] = new APMValueProperty(entry.DefaultValue, entry.CurrentValue);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
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
