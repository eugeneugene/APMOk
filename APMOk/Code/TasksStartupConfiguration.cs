using CustomConfiguration;
using Microsoft.Extensions.Configuration;
using System;

namespace APMOk
{
    public class TasksStartupConfiguration : ICustomConfiguration
    {
        public IConfiguration Configuration { get; }
        public string SectionName => "TasksStartup";

        public IConfigurationParameter<ITaskStartup> BatteryStatusReader { get; }
        public IConfigurationParameter<ITaskStartup> DiskStatusReader { get; }

        public TasksStartupConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
            BatteryStatusReader = ConfigurationParameterFactory.CreateParameter(Configuration, nameof(BatteryStatusReader), SectionName + ":" + nameof(BatteryStatusReader),
                new TaskStartupParameterDecorator<ITaskStartup>(new DefaultTaskStartup(TimeSpan.FromSeconds(3))), "Периодичность чтения статуса батареи");
            DiskStatusReader = ConfigurationParameterFactory.CreateParameter(Configuration, nameof(DiskStatusReader), SectionName + ":" + nameof(DiskStatusReader),
                new TaskStartupParameterDecorator<ITaskStartup>(new DefaultTaskStartup(TimeSpan.FromMinutes(1))), "Периодичность чтения информации о жёстких дисках");        
        }
    }
}
