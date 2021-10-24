using APMData;
using APMOk.Models;
using APMOkLib.RecurrentTasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Tasks
{
    public class DiskInfoReaderTask : IRunnable
    {
        private readonly APMOkModel _apmOkModel;

        public DiskInfoReaderTask(APMOkModel apmOkModel)
        {
            _apmOkModel = apmOkModel;
        }

        public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var diskInfoService = scopeServiceProvider.GetRequiredService<Services.DiskInfo>();
                var configurationService = scopeServiceProvider.GetRequiredService<Services.Configuration>();
                var apmService = scopeServiceProvider.GetRequiredService<Services.APM>();

                var systemDiskInfoReply = await diskInfoService.EnumerateDisksAsync(cancellationToken);
                if (systemDiskInfoReply is null)
                    throw new Exception("Service is offline");

                var driveAPMConfigurationReply = await configurationService.GetDriveAPMConfigurationAsync(cancellationToken);
                if (driveAPMConfigurationReply is null)
                    throw new Exception("Service is offline");

                _apmOkModel.ConnectFailure = false;
                _apmOkModel.SystemDiskInfo = systemDiskInfoReply;

                foreach (var entry in systemDiskInfoReply.DiskInfoEntries)
                {
                    var currentAPM = await apmService.GetCurrentAPMAsync(new()
                    {
                        DeviceID = entry.DeviceID,
                    }, cancellationToken);

                    DriveAPMConfigurationReplyEntry? driveAPMConfiguration = null;
                    if (driveAPMConfigurationReply.ReplyResult != 0)
                        driveAPMConfiguration = driveAPMConfigurationReply.DriveAPMConfigurationReplyEntries.SingleOrDefault(item => item.DeviceID == entry.DeviceID);
                    if (driveAPMConfiguration is null)
                    {
                        driveAPMConfiguration = new DriveAPMConfigurationReplyEntry
                        {
                            DeviceID = entry.DeviceID,
                        };
                    }
                    _apmOkModel.UpdateAPMValue(entry.DeviceID, new APMValueProperty(onMains: driveAPMConfiguration.OnMains, onBatteries: driveAPMConfiguration.OnBatteries, current: currentAPM.APMValue));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _apmOkModel.ConnectFailure = true;
            }
        }
    }
}
