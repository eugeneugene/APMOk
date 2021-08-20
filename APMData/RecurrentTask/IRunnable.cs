using System;
using System.Threading;
using System.Threading.Tasks;

namespace RecurrentTasks
{
    [Obsolete("Устаревшаяя реализация. Обновить из проекта SBCore")]
    public interface IRunnable
    {
        Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken);
    }
}
