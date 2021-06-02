using Microsoft.Extensions.DependencyInjection;
using RecurrentTasks;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Tasks
{
    public class BatteryStatusReaderTask : IRunnable
    {
        private readonly APMOkModel _apmOkData;

        public BatteryStatusReaderTask(APMOkModel apmOkData)
        {
            _apmOkData = apmOkData;
        }

        public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var diskInfoService = scopeServiceProvider.GetRequiredService<Services.PowerStateService>();
                var reply = await diskInfoService.GetPowerStateAsync(cancellationToken);
                _apmOkData.PowerState = reply ?? throw new Exception("Service is offline");
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
