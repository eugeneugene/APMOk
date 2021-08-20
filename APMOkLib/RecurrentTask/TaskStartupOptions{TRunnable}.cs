using System;

namespace APMOkLib.RecurrentTasks
{
    public class TaskStartupOptions<TRunnable> where TRunnable : IRunnable
    {
        public TaskStartupOptions(IServiceProvider serviceProvider, TaskOptions<TRunnable> taskOptions)
        {
            ServiceProvider = serviceProvider;
            TaskOptions = taskOptions;
        }

        public IServiceProvider ServiceProvider { get; }
        public TaskOptions<TRunnable> TaskOptions { get; }
    }
}
