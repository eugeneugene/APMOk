using APMData.Proto;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Tasks
{
    public class NotificationIconUpdaterTask : IHostedService
    {
        private readonly APMOkData _data;
        private readonly TaskbarIcon _taskbarIcon;

        //private bool IgnoreUpdate = false;

        public NotificationIconUpdaterTask(APMOkData data, TaskbarIcon taskbarIcon)
        {
            _data = data;
            _taskbarIcon = taskbarIcon;
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
            UpdateIcon();
        }

        private void UpdateIcon()
        {
            if (_data.ConnectFailure)
                _taskbarIcon.Icon = Properties.Resources.Error;
            else
            {
                var ACLineStatus = _data.PowerState?.ACLineStatus ?? EACLineStatus.LineStatusUnknown;
                _taskbarIcon.Icon = ACLineStatus switch
                {
                    EACLineStatus.Online => Properties.Resources.Checked,
                    EACLineStatus.Offline => Properties.Resources.Battery,
                    _ => Properties.Resources.Error
                };
            }
        }
    }
}
