﻿using APMOk.Tasks;
using APMOkLib.RecurrentTasks;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Services
{
    public class NotificationIconUpdaterTask : IHostedService
    {
        private readonly APMOkModel _data;
        private readonly TaskbarIcon _taskbarIcon;
        private readonly ITask<DiskInfoReaderTask> _diskInfoReaderTask;
        private readonly ITask<PowerStatusReaderTask> _batteryStatusReaderTask;

        //private bool IgnoreUpdate = false;

        public NotificationIconUpdaterTask(APMOkModel data, TaskbarIcon taskbarIcon, ITask<DiskInfoReaderTask> diskInfoReaderTask, ITask<PowerStatusReaderTask> batteryStatusReaderTask)
        {
            _data = data;
            _taskbarIcon = taskbarIcon;
            _diskInfoReaderTask = diskInfoReaderTask;
            _batteryStatusReaderTask = batteryStatusReaderTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            UpdateIcon();
            _data.PropertyChanged += APMOkDataPropertyChanged;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _data.PropertyChanged -= APMOkDataPropertyChanged;
            return Task.CompletedTask;
        }

        private void APMOkDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ConnectFailure" && _data.ConnectFailure == false)
            {
                if (_data.SystemDiskInfo == null && _diskInfoReaderTask.IsStarted)
                    _diskInfoReaderTask.TryRunImmediately();
                if (_data.PowerState == null && _batteryStatusReaderTask.IsStarted)
                    _batteryStatusReaderTask.TryRunImmediately();
            }
            UpdateIcon();
        }

        private void UpdateIcon()
        {
            if (_data.ConnectFailure)
                _taskbarIcon.Icon = Properties.Resources.Error;
            else
            {

                var PowerSource = APMData.Proto.EPowerSource.Unknown;
                if (_data.PowerState.ReplyResult == 1)
                    PowerSource = _data.PowerState?.PowerState.PowerSource ?? APMData.Proto.EPowerSource.Unknown;
                _taskbarIcon.Dispatcher.Invoke(() =>
                {
                    switch (PowerSource)
                    {
                        case APMData.Proto.EPowerSource.Mains:
                            _taskbarIcon.Icon = Properties.Resources.Checked;
                            _taskbarIcon.ToolTipText = "Online";
                            break;
                        case APMData.Proto.EPowerSource.Battery:
                            _taskbarIcon.Icon = Properties.Resources.Battery;
                            _taskbarIcon.ToolTipText = "Offline";
                            break;
                        default:
                            _taskbarIcon.Icon = Properties.Resources.Error;
                            _taskbarIcon.ToolTipText = "Error";
                            break;
                    }
                });
            }
        }
    }
}
