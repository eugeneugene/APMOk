namespace APMOkLib.CustomConfiguration
{

    public interface IConfigurationParameter<T> : IConfigurationParameter
    {
        T? Value { get; }
        T? DefaultValue { get; }
    }
}
