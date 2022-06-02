using APMOkLib.CustomConfiguration;
using Microsoft.Extensions.Configuration;
using System;

namespace APMOk.Code;

public class TasksStartupConfiguration : SmartConfiguration, ITasksStartupConfiguration
{
    public IConfigurationParameter<ITaskStartup> PowerStatusReader { get; }
    public IConfigurationParameter<ITaskStartup> DiskStatusReader { get; }

    public TasksStartupConfiguration(IConfiguration configuration, ConfigurationParameterFactory parameterFactory)
        : base("TasksStartup", configuration)
    {
        if (parameterFactory is null)
            throw new ArgumentNullException(nameof(parameterFactory));

        PowerStatusReader = parameterFactory.CreateParameter(this, nameof(PowerStatusReader), nameof(PowerStatusReader),
            new TaskStartupParameterDecorator<ITaskStartup>(new TaskStartupParameter(TimeSpan.FromSeconds(1))), "Battery status read delay");
        DiskStatusReader = parameterFactory.CreateParameter(this, nameof(DiskStatusReader), nameof(DiskStatusReader),
            new TaskStartupParameterDecorator<ITaskStartup>(new TaskStartupParameter(TimeSpan.FromMinutes(1))), "Harddrives' information read delay");
    }
}
