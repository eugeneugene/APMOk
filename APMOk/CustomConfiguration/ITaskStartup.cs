using System;

namespace CustomConfiguration
{
    public interface ITaskStartup
    {
        TimeSpan Interval { get; }
        TimeSpan FirstRunDelay { get; }
    }
}
