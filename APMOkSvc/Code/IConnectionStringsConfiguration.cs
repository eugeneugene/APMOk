using APMOkLib.CustomConfiguration;

namespace APMOkSvc.Code;

public interface IConnectionStringsConfiguration
{
    IConfigurationParameter<string> DataContext { get; }
}
