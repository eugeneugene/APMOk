using Microsoft.Extensions.DependencyInjection;
using RecurrentTasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    public class PowerStatusReaderTask : IRunnable
    {
        public Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            var powerStateServiceImpl = scopeServiceProvider.GetRequiredService<PowerStateServiceImpl>();
            var powerStateReply = powerStateServiceImpl.GetPowerState();
            var powerStatusContainer = scopeServiceProvider.GetRequiredService<PowerStatusContainer>();
            powerStatusContainer.PowerState = powerStateReply;

            return Task.CompletedTask;
        }
    }
}
