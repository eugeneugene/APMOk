using System;

namespace APMOkLib.RecurrentTasks;

public static class TaskOptionsExtensions
{
    public static TaskOptions AutoStart(this TaskOptions taskOptions, TimeSpan interval)
    {
        if (taskOptions is null)
            throw new ArgumentNullException(nameof(taskOptions));
        taskOptions.Interval = interval;
        return taskOptions;
    }

    public static TaskOptions AutoStart(this TaskOptions taskOptions, uint interval)
    {
        return AutoStart(taskOptions, TimeSpan.FromSeconds(interval));
    }

    public static TaskOptions AutoStart(this TaskOptions taskOptions, TimeSpan interval, TimeSpan firstRunDelay)
    {
        if (taskOptions is null)
            throw new ArgumentNullException(nameof(taskOptions));
        taskOptions.Interval = interval;
        taskOptions.FirstRunDelay = firstRunDelay;
        return taskOptions;
    }
}
