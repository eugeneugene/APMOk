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
        private readonly APMOkData _apmOkData;

        public BatteryStatusReaderTask(APMOkData apmOkData)
        {
            _apmOkData = apmOkData;
        }

        public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            try
            {
                var diskInfoService = scopeServiceProvider.GetRequiredService<Services.PowerStateService>();
                var reply = await diskInfoService.GetPowerStateAsync();
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
