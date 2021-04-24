using System;
using System.Threading;
using System.Threading.Tasks;

namespace RecurrentTasks
{
    public interface IRunnable
    {
        Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken);
    }
}
