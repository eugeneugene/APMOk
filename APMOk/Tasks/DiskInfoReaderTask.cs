using APMOk.Code;
using Microsoft.Extensions.DependencyInjection;
using RecurrentTasks;
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

                var diskInfoService = scopeServiceProvider.GetRequiredService<Services.DiskInfoService>();
                var configurationService = scopeServiceProvider.GetRequiredService<Services.ConfigurationService>();

                var systemDiskInfoReply = await diskInfoService.EnumerateDisksAsync(cancellationToken);
                _apmOkData.SystemDiskInfo = systemDiskInfoReply ?? throw new Exception("Service is offline");

                var driveAPMConfigurationReply = await configurationService.GetDriveAPMConfigurationAsync(cancellationToken);
                if (driveAPMConfigurationReply == null)
                    throw new Exception("Service is offline");

                if (driveAPMConfigurationReply.ReplyResult != 0)
                {
                    foreach (var entry in driveAPMConfigurationReply.DriveAPMConfigurationReplyEntries)
                        _apmOkData.APMValueDictionary[entry.DeviceID] = new APMValueProperty(defaultValue: (int)entry.DefaultValue,
                                                                                             userValue: (int)entry.UserValue,
                                                                                             currentValue: 0);
                }

                _apmOkData.ConnectFailure = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _apmOkData.ConnectFailure = true;
            }
        }
    }
}
