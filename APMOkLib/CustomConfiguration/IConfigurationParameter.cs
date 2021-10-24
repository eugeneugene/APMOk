namespace APMOkLib.CustomConfiguration
{
    public interface IConfigurationParameter
    {
        string? StringValue { get; }
        string? StringDefaultValue { get; }
        bool Exists { get; }
        string Name { get; }
        string Section { get; }
        string Description { get; }
    }
}
