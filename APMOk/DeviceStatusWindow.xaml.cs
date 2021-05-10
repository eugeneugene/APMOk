﻿using APMData.Proto;
using APMOkLib;
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
        private bool disposedValue;

        public ObservableCollection<DiskInfoEntry> DiskInfo { get; } = new();

        public ObservableCollection<KeyValuePair<string, object>> DiskInfoItems { get; } = new();

        public DeviceStatusWindow(APMOkData apmOkData)
        {
            InitializeComponent();

            _apmOkData = apmOkData;

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
            LoadDisksInfo(_apmOkData.SystemDiskInfo);
            LoadPowerStatus(_apmOkData.PowerState);
            _apmOkData.PropertyChanged += APMOkDataPropertyChanged;
        }

        private void APMOkDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SystemDiskInfo":
                    LoadDisksInfo(_apmOkData.SystemDiskInfo);
                    break;
                case "PowerState":
                    LoadPowerStatus(_apmOkData.PowerState);
                    break;
                case "ConnectFailure":
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void LoadDisksInfo(SystemDiskInfoReply reply)
        {
            Dispatcher.Invoke(() =>
            {
                var selectedItem = SelectDiskCombo.SelectedItem as DiskInfoEntry;
                string selectedDevice = selectedItem?.DeviceID;
                DiskInfo.Clear();
                if (reply != null)
                {
                    if (reply.ReplyResult != 0)
                    {
                        foreach (var entry in reply.DiskInfoEntries.Where(item => item.InfoValid))
                            DiskInfo.Add(entry);

                        if (DiskInfo.Any())
                        {
                            if (!string.IsNullOrEmpty(selectedDevice))
                                SelectDiskCombo.SelectedItem = DiskInfo.SingleOrDefault(item => item.DeviceID == selectedDevice);
                            else
                                SelectDiskCombo.SelectedIndex = 0;
                        }
                        else
                            SelectDiskCombo.SelectedIndex = -1;
                    }
                }
            });
        }

        private void LoadPowerStatus(PowerStateReply reply)
        {
            if (reply == null)
                _apmOkData.PowerState = PowerStateReply.FailureReply;
            else
                _apmOkData.PowerState = reply;
        }

        private void SelectDiskComboSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            Dispatcher.Invoke(() =>
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
        }
    }
}
