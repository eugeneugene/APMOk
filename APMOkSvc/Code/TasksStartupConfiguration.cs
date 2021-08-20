using APMOkLib.CustomConfiguration;
using Microsoft.Extensions.Configuration;
using System;

namespace APMOkSvc.Code
{
    public class TasksStartupConfiguration : SmartConfiguration, ITasksStartupConfiguration
    {
        public IConfigurationParameter<ITaskStartup> PowerStatusReader { get; }

        public TasksStartupConfiguration(IConfiguration configuration, ConfigurationParameterFactory parameterFactory)
            : base("TasksStartup", configuration)
        {
            PowerStatusReader = parameterFactory.CreateParameter(this, nameof(PowerStatusReader), nameof(PowerStatusReader),
                new TaskStartupParameterDecorator<ITaskStartup>(new TaskStartupParameter(TimeSpan.FromSeconds(0.5))), "Периодичность чтения статуса батареи");
        }
    }
}
