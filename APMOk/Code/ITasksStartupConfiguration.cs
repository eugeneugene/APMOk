using APMOkLib.CustomConfiguration;

namespace APMOk.Code;

public interface ITasksStartupConfiguration
{
    IConfigurationParameter<ITaskStartup> DiskStatusReader { get; }
    IConfigurationParameter<ITaskStartup> PowerStatusReader { get; }
}
