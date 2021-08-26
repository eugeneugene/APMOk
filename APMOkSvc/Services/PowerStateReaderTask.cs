using APMOkLib.RecurrentTasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    public class PowerStateReaderTask : IRunnable
    {
        public Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            var powerStateServiceImpl = scopeServiceProvider.GetRequiredService<PowerStateServiceImpl>();
            var powerStateReply = powerStateServiceImpl.GetPowerState();
            var powerStatusContainer = scopeServiceProvider.GetRequiredService<PowerStateContainer>();
            powerStatusContainer.PowerState = powerStateReply;

            return Task.CompletedTask;
        }
    }
}
