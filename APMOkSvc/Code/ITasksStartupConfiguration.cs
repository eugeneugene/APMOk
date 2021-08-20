using APMOkLib.CustomConfiguration;

namespace APMOkSvc.Code
{
    public interface ITasksStartupConfiguration
    {
        IConfigurationParameter<ITaskStartup> PowerStatusReader { get; }
    }
}