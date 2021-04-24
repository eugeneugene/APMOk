using RecurrentTasks;
using System;

namespace CustomConfiguration
{
    public static class TaskOptionsExtensionsTaskStartup
    {
        public static TaskOptions AutoStart(this TaskOptions taskOptions, ITaskStartup taskStartup)
        {
            if (taskOptions == null)
                throw new ArgumentNullException(nameof(taskOptions));
            taskOptions.Interval = taskStartup.Interval;
            taskOptions.FirstRunDelay = taskStartup.FirstRunDelay;
            return taskOptions;
        }
    }
}
