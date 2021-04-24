using Grpc.Core;
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
        private readonly ACLineStatus _powerState;

        public BatteryStatusReaderTask(ACLineStatus powerState)
        {
            _powerState = powerState;
        }

        public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            try
            {
                using var diskInfoService = scopeServiceProvider.GetRequiredService<Services.PowerStateService>();
                var reply = await diskInfoService.GetPowerStateAsync();
                ViewModel.Battery = (ACLineStatus)reply.ACLineStatus;
            }
            catch (RpcException rex)
            {
                ViewModel.Battery = ACLineStatus.Error;
                Debug.WriteLine(rex.Message);
            }
        }
    }
}
