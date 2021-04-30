namespace CustomConfiguration
{
    public interface IConfigurationParameter<T>
    {
        T Value { get; }
        bool Exists { get; }

        string Section { get; }
        string Description { get; }
    }
}
