using APMOk.Models;
using APMOkLib.RecurrentTasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace APMOk.Tasks
{
    internal class PowerStatusReaderTask : IRunnable
    {
        private readonly APMOkModel _apmOkData;

        public PowerStatusReaderTask(APMOkModel apmOkData)
        {
            _apmOkData = apmOkData;
        }

        public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var diskInfoService = scopeServiceProvider.GetRequiredService<Services.PowerState>();
                var reply = await diskInfoService.GetPowerStateAsync(cancellationToken);

                if (reply is null)
                    throw new Exception("Service is offline");

                _apmOkData.ConnectFailure = false;
                _apmOkData.PowerState = reply;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _apmOkData.ConnectFailure = true;
            }
        }
    }
}
