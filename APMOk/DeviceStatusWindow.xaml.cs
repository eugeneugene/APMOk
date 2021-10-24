using APMData;
using APMOk.Code;
using APMOk.Models;
using APMOk.Tasks;
using APMOkLib;
using APMOkLib.RecurrentTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly APMOkModel _apmOkModel;
        private readonly IServiceProvider _scopeServiceProvider;
        private bool disposedValue;

        private static readonly ImageSource Error = Properties.Resources.Error.ToImageSource();
        private static readonly ImageSource Checked = Properties.Resources.Checked.ToImageSource();
        private static readonly ImageSource Battery = Properties.Resources.Battery.ToImageSource();

        public ObservableCollection<DiskInfoEntry> DiskInfo { get; } = new();

        public ObservableConcurrentDictionary<string, object> DiskInfoItems { get; } = new();

        public ObservableConcurrentDictionary<string, object> PowerStateItems { get; } = new();

        public APMValueProperty? APMValue { get; }

        public DeviceStatusWindow(IServiceProvider scopeServiceProvider, APMOkModel apmOkModel)
        {
            InitializeComponent();

            _scopeServiceProvider = scopeServiceProvider;
            _apmOkModel = apmOkModel;

            CollectionViewSource? deviceStatusDataSource = FindResource("DeviceStatusDataSource") as CollectionViewSource;
            if (deviceStatusDataSource is not null)
                deviceStatusDataSource.Source = DiskInfo;

            DriveStatusGrid.DataContext = DiskInfoItems;
            BatteryStatusGrid.DataContext = PowerStateItems;

            DataContext = _apmOkModel;

            APMValue = FindResource("APMValue") as APMValueProperty;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadDisksInfo();
            LoadPowerState();
            UpdateIcon();
            _apmOkModel.PropertyChanged += APMOkDataChanged;

            var software = Registry.CurrentUser.OpenSubKey("SOFTWARE", false);
            var microsoft = software?.OpenSubKey("Microsoft", false);
            var windows = microsoft?.OpenSubKey("Windows", false);
            var currentVersion = windows?.OpenSubKey("CurrentVersion", false);
            var run = currentVersion?.OpenSubKey("Run", true);
            if (run?.GetValue("APMOk") is not null)
                AutoRun.IsChecked = true;
        }

        private void APMOkDataChanged(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"APMOkModel has changed: {e.PropertyName}");
            Debug.WriteLine("{0}", _apmOkModel);

            switch (e.PropertyName)
            {
                case "SystemDiskInfo":
                    LoadDisksInfo();
                    break;
                case "PowerState":
                    LoadPowerState();
                    RunDiskInfoReaderTask();
                    UpdateIcon();
                    break;
                case "ConnectFailure":
                    UpdateIcon();
                    break;
                case "APMValueDictionary":
                    break;
                default:
                    throw new NotImplementedException($"In {nameof(DeviceStatusWindow)}");
            }
            LoadAPMValue();
        }

        private void APMValueChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"APMValue has changed: {e.PropertyName}");
            Debug.WriteLine("{0}", APMValue);
        }

        private void LoadPowerState()
        {
            if (_apmOkModel.PowerState is not null && _apmOkModel.PowerState.ReplyResult != 0)
            {
                PowerStateItems[nameof(_apmOkModel.PowerState.PowerState.PowerSource)] = _apmOkModel.PowerState.PowerState.PowerSource;
                PowerStateItems[nameof(_apmOkModel.PowerState.PowerState.BatteryFlag)] = _apmOkModel.PowerState.PowerState.BatteryFlag;
                PowerStateItems[nameof(_apmOkModel.PowerState.PowerState.BatteryFullLifeTime)] = _apmOkModel.PowerState.PowerState.BatteryFullLifeTime;
                PowerStateItems[nameof(_apmOkModel.PowerState.PowerState.BatteryLifePercent)] = _apmOkModel.PowerState.PowerState.BatteryLifePercent;
                PowerStateItems[nameof(_apmOkModel.PowerState.PowerState.BatteryLifeTime)] = _apmOkModel.PowerState.PowerState.BatteryLifeTime;
            }
        }

        private void LoadDisksInfo()
        {
            if (_apmOkModel.SystemDiskInfo is null || _apmOkModel.SystemDiskInfo.ReplyResult == 0)
                return;

            Dispatcher.Invoke(() =>
            {
                DiskInfo.Clear();

                foreach (var entry in _apmOkModel.SystemDiskInfo.DiskInfoEntries.OrderBy(item => item.DeviceID))
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
            var deviceId = DiskInfoItems[nameof(DiskInfoEntry.DeviceID)] as string;
            var value = _apmOkModel.GetAPMValue(deviceId);
            if (APMValue is not null && value is not null)
            {
                Debug.WriteLine($"APMValue {value}");
                APMValue.OnMains = value.OnMains;
                APMValue.OnBatteries = value.OnBatteries;
                APMValue.Current = value.Current;
            }
            else
                Debug.WriteLine("APMValue is null");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _apmOkModel.PropertyChanged -= APMOkDataChanged;
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
            if (_apmOkModel.SystemDiskInfo is null || _apmOkModel.SystemDiskInfo.ReplyResult == 0)
                return;

            string? selectedSerial = null;
            if (SelectDiskCombo.SelectedItem is DiskInfoEntry selectedItem)
                selectedSerial = selectedItem.SerialNumber;

            // Items to delete from combobox
            var deleteItems = DiskInfo.Where(item => !_apmOkModel.SystemDiskInfo.DiskInfoEntries.Any(item1 => item1.Equals(item)));
            foreach (var item in deleteItems)
                DiskInfo.Remove(item);

            // Items to add to combobox
            var addItems = _apmOkModel.SystemDiskInfo.DiskInfoEntries.Where(item => !DiskInfo.Any(item1 => item1.Equals(item)));
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
                if (_apmOkModel.SystemDiskInfo.DiskInfoEntries.Any())
                    SelectDiskCombo.SelectedIndex = 0;
                else
                    SelectDiskCombo.SelectedIndex = -1;
            }
        }

        private void UpdateIcon()
        {
            if (_apmOkModel.ConnectFailure)
                Dispatcher.Invoke(() => Icon = Error);
            else
            {
                EPowerSource PowerSource = EPowerSource.Unknown;
                if (_apmOkModel.PowerState is not null && _apmOkModel.PowerState.ReplyResult == 1)
                    PowerSource = _apmOkModel.PowerState.PowerState.PowerSource;
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

        private void SetAPMValueMenuCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = APMValue is not null && APMValue.Current != 0;
            e.Handled = true;
        }

        private void SetAPMValueMenuExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            var cm = TryFindResource("SetApmValueContextMenu") as ContextMenu;
            if (cm is not null)
                cm.IsOpen = true;
        }

        private void SetAPMValueCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = APMValue is not null && APMValue.Current != 0;
            e.Handled = true;
        }

        private async void SetAPMValueExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Parameter is APMCommandParameter parameter)
                    await SetAPMValueAsync(parameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: {0}", ex.Message);
            }
            e.Handled = true;
        }

        private async Task SetAPMValueAsync(APMCommandParameter parameter)
        {
            uint ApmValue = parameter.ApmValue;
            EPowerSource powerSource = parameter.PowerSource;

            var deviceId = DiskInfoItems[nameof(DiskInfoEntry.DeviceID)] as string;
            bool result;
            if (ApmValue == 0U)
            {
                var configuration = _scopeServiceProvider.GetRequiredService<Services.Configuration>();
                var res = await configuration.ResetDriveAPMConfigurationAsync(new() { DeviceID = deviceId, PowerSource = powerSource }, CancellationToken.None);
                result = (res?.ReplyResult ?? 0) != 0;
            }
            else
            {
                var apm = _scopeServiceProvider.GetRequiredService<Services.APM>();
                var res = await apm.SetAPMAsync(new() { DeviceID = deviceId, APMValue = ApmValue, PowerSource = powerSource }, CancellationToken.None);
                result = (res?.ReplyResult ?? 0) != 0;
            }

            if (result)
                RunDiskInfoReaderTask();
        }

        private void RunDiskInfoReaderTask()
        {
            var diskInfoReaderTask = _scopeServiceProvider.GetRequiredService<ITask<DiskInfoReaderTask>>();
            if (diskInfoReaderTask.IsStarted && !diskInfoReaderTask.IsRunningRightNow)
                diskInfoReaderTask.TryRunImmediately();
        }

        private async void SetAPMCustomValueMenuExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Parameter is APMCustomValueCommandParameter parameter)
                {
                    uint value = parameter.PowerSource switch
                    {
                        EPowerSource.Mains => APMValue?.OnMains ?? 0U,
                        EPowerSource.Battery => APMValue?.OnBatteries ?? 0U,
                        _ => 0U,
                    };
                    var dlg = new SetAPMCustomValueWindow(new APMCommandParameter()
                    {
                        ApmValue = value,
                        PowerSource = parameter.PowerSource,
                    });
                    if (dlg.ShowDialog() ?? false)
                    {
                        await SetAPMValueAsync(new APMCommandParameter()
                        {
                            ApmValue = dlg.APMCommandParameter.ApmValue,
                            PowerSource = dlg.APMCommandParameter.PowerSource,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: {0}", ex.Message);
            }
            e.Handled = true;
        }

        private void SetAPMCustomValueMenuCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = APMValue is not null && APMValue.Current != 0;
            e.Handled = true;
        }

        private void AutoRunChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var software = Registry.CurrentUser.OpenSubKey("SOFTWARE", false);
                var microsoft = software?.OpenSubKey("Microsoft", false);
                var windows = microsoft?.OpenSubKey("Windows", false);
                var currentVersion = windows?.OpenSubKey("CurrentVersion", false);
                var run = currentVersion?.OpenSubKey("Run", true);
                if (run is not null)
                {
                    if (Path.GetExtension(strExeFilePath) == ".dll")
                        strExeFilePath = Path.ChangeExtension(strExeFilePath, ".exe");
                    if (strExeFilePath.Contains(' '))
                        strExeFilePath = @"""" + strExeFilePath + @"""";
                    run.SetValue("APMOk", strExeFilePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: {0}", ex.Message);
            }
            e.Handled = true;
        }

        private void AutoRunUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var software = Registry.CurrentUser.OpenSubKey("SOFTWARE", false);
                var microsoft = software?.OpenSubKey("Microsoft", false);
                var windows = microsoft?.OpenSubKey("Windows", false);
                var currentVersion = windows?.OpenSubKey("CurrentVersion", false);
                var run = currentVersion?.OpenSubKey("Run", true);
                run?.DeleteValue("APMOk");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: {0}", ex.Message);
            }
            e.Handled = true;
        }
    }
}
