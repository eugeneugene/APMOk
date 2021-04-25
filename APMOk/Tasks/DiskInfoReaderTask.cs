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
        private readonly APMOkData _apmOkData;

        public DiskInfoReaderTask(APMOkData apmOkData)
        {
            _apmOkData = apmOkData;
        }

        public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            try
            {
                using var diskInfoService = scopeServiceProvider.GetRequiredService<Services.DiskInfoService>();
                var reply = await diskInfoService.EnumerateDisksAsync();
                _apmOkData.SystemDiskInfo = reply ?? throw new Exception("Service is offline");
                Debug.WriteLine(_apmOkData.SystemDiskInfo);
            }
            catch (Exception rex)
            {
                Debug.WriteLine(rex.Message);
                _apmOkData.PowerState = null;
                _apmOkData.SystemDiskInfo = null;
            }
        }
    }
}
