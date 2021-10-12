using APMOk.Models;
using APMOkLib.RecurrentTasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Tasks
{
    public class DiskInfoReaderTask : IRunnable
    {
        private readonly APMOkModel _apmOkData;

        public DiskInfoReaderTask(APMOkModel apmOkData)
        {
            _apmOkData = apmOkData;
        }

        public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var diskInfoService = scopeServiceProvider.GetRequiredService<Services.DiskInfo>();
                var configurationService = scopeServiceProvider.GetRequiredService<Services.Configuration>();

                var systemDiskInfoReply = await diskInfoService.EnumerateDisksAsync(cancellationToken);
                if (systemDiskInfoReply is null)
                    throw new Exception("Service is offline");

                var driveAPMConfigurationReply = await configurationService.GetDriveAPMConfigurationAsync(cancellationToken);
                if (driveAPMConfigurationReply is null)
                    throw new Exception("Service is offline");

                _apmOkData.ConnectFailure = false;
                _apmOkData.SystemDiskInfo = systemDiskInfoReply;

                if (driveAPMConfigurationReply.ReplyResult != 0)
                {
                    foreach (var entry in driveAPMConfigurationReply.DriveAPMConfigurationReplyEntries)
                        _apmOkData.APMValueDictionary[entry.DeviceID] = new APMValueProperty(onMains: entry.OnMains, onBatteries: entry.OnBatteries);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _apmOkData.ConnectFailure = true;
            }
        }
    }
}
