﻿using APMData;
using APMOk.Code;
using APMOk.Models;
using APMOkLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace APMOk
{
    /// <summary>
    /// Interaction logic for DeviceStatusWindow.xaml
    /// </summary>
    internal partial class DeviceStatusWindow : Window, IDisposable
    {
        private readonly APMOkModel _apmOkData;
        private bool disposedValue;

        private static readonly ImageSource Error = Properties.Resources.Error.ToImageSource();
        private static readonly ImageSource Checked = Properties.Resources.Checked.ToImageSource();
        private static readonly ImageSource Battery = Properties.Resources.Battery.ToImageSource();

        public ObservableCollection<DiskInfoEntry> DiskInfo { get; } = new();

        public ObservableConcurrentDictionary<string, object> DiskInfoItems { get; } = new();

        public ObservableConcurrentDictionary<string, object> PowerStateItems { get; } = new();

        public APMValueProperty APMValue { get; }

        public DeviceStatusWindow(APMOkModel apmOkData)
        {
            InitializeComponent();

            _apmOkData = apmOkData;

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
                case "ConnectFailure":
                    LoadPowerState();
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
            if (_apmOkData.PowerState is not null && _apmOkData.PowerState.ReplyResult != 0)
            {
                PowerStateItems[nameof(_apmOkData.PowerState.PowerState.PowerSource)] = _apmOkData.PowerState.PowerState.PowerSource;
                PowerStateItems[nameof(_apmOkData.PowerState.PowerState.BatteryFlag)] = _apmOkData.PowerState.PowerState.BatteryFlag;
                PowerStateItems[nameof(_apmOkData.PowerState.PowerState.BatteryFullLifeTime)] = _apmOkData.PowerState.PowerState.BatteryFullLifeTime;
                PowerStateItems[nameof(_apmOkData.PowerState.PowerState.BatteryLifePercent)] = _apmOkData.PowerState.PowerState.BatteryLifePercent;
                PowerStateItems[nameof(_apmOkData.PowerState.PowerState.BatteryLifeTime)] = _apmOkData.PowerState.PowerState.BatteryLifeTime;
            }
            UpdateIcon();
        }

        private void LoadDisksInfo()
        {
            if (_apmOkData.SystemDiskInfo is null || _apmOkData.SystemDiskInfo.ReplyResult == 0)
                return;

            Dispatcher.Invoke(() =>
            {
                DiskInfo.Clear();

                foreach (var entry in _apmOkData.SystemDiskInfo.DiskInfoEntries.OrderBy(item => item.DeviceID))
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
                    APMValue.OnMains = device.Value.OnMains;
                    APMValue.OnBatteries = device.Value.OnBatteries;
                    APMValue.Current = device.Value.Current;
                }
                else
                {
                    APMValue.OnMains = 0U;
                    APMValue.OnBatteries = 0U;
                    APMValue.Current = 0U;
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
            if (_apmOkData.SystemDiskInfo is null || _apmOkData.SystemDiskInfo.ReplyResult == 0)
                return;

            string selectedSerial = null;
            if (SelectDiskCombo.SelectedItem is DiskInfoEntry selectedItem)
                selectedSerial = selectedItem.SerialNumber;

            // Items to delete from combobox
            var deleteItems = DiskInfo.Where(item => !_apmOkData.SystemDiskInfo.DiskInfoEntries.Any(item1 => item1.Equals(item)));
            foreach (var item in deleteItems)
                DiskInfo.Remove(item);

            // Items to add to combobox
            var addItems = _apmOkData.SystemDiskInfo.DiskInfoEntries.Where(item => !DiskInfo.Any(item1 => item1.Equals(item)));
            foreach (var item in addItems)
                DiskInfo.Add(item);

            List<DiskInfoEntry> DiskInfoTmp = new();
            DiskInfoTmp.AddRange(DiskInfo);

            DiskInfo.Clear();
            foreach (var item in DiskInfoTmp.OrderBy(item => item.DeviceID))
                DiskInfo.Add(item);

            var selectItem = DiskInfo.SingleOrDefault(item => item.SerialNumber == selectedSerial);
            if (selectItem is not null)
                SelectDiskCombo.SelectedItem = selectItem;
            else
            {
                if (_apmOkData.SystemDiskInfo.DiskInfoEntries.Any())
                    SelectDiskCombo.SelectedIndex = 0;
                else
                    SelectDiskCombo.SelectedIndex = -1;
            }
        }

        private void UpdateIcon()
        {
            if (_apmOkData.ConnectFailure)
                Dispatcher.Invoke(() => Icon = Error);
            else
            {
                EPowerSource PowerSource = EPowerSource.Unknown;
                if (_apmOkData.PowerState is not null && _apmOkData.PowerState.ReplyResult == 1)
                    PowerSource = _apmOkData.PowerState.PowerState.PowerSource;
                Dispatcher.Invoke(() =>
                {
                    Icon = PowerSource switch
                    {
                        EPowerSource.Mains => Checked,
                        EPowerSource.Battery => Battery,
                        _ => Error,
                    };
                });
            }
        }

        private void SetAPMValueCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = APMValue is not null && APMValue.Current != 0;
            e.Handled = true;
        }

        private void SetAPMValueExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            var cm = TryFindResource("SetApmValueContextMenu") as ContextMenu;
            if (cm is not null)
                cm.IsOpen = true;
        }
    }
}
