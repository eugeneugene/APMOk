using APMOkLib.RecurrentTasks;
using System;

namespace APMOkLib.CustomConfiguration
{
    public static class TaskOptionsExtensionsTaskStartup
    {
        public static TaskOptions AutoStart(this TaskOptions taskOptions, ITaskStartup taskStartup)
        {
            if (taskOptions is null)
                throw new ArgumentNullException(nameof(taskOptions));
            taskOptions.Interval = taskStartup.Interval;
            taskOptions.FirstRunDelay = taskStartup.FirstRunDelay;
            return taskOptions;
        }
    }
}
