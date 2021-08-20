using CustomConfiguration;
using Microsoft.Extensions.Configuration;
using System;

namespace APMOk
{
    public class TasksStartupConfiguration : ICustomConfiguration
    {
        public IConfiguration Configuration { get; }
        public string SectionName => "TasksStartup";

        public IConfigurationParameter<ITaskStartup> PowerStatusReader { get; }
        public IConfigurationParameter<ITaskStartup> DiskStatusReader { get; }

        public TasksStartupConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
            PowerStatusReader = ConfigurationParameterFactory.CreateParameter(Configuration, nameof(PowerStatusReader), SectionName + ":" + nameof(PowerStatusReader),
                new TaskStartupParameterDecorator<ITaskStartup>(new DefaultTaskStartup(TimeSpan.FromSeconds(1))), "Периодичность чтения статуса батареи");
            DiskStatusReader = ConfigurationParameterFactory.CreateParameter(Configuration, nameof(DiskStatusReader), SectionName + ":" + nameof(DiskStatusReader),
                new TaskStartupParameterDecorator<ITaskStartup>(new DefaultTaskStartup(TimeSpan.FromMinutes(1))), "Периодичность чтения информации о жёстких дисках");        
        }
    }
}
