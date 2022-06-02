using System;

namespace APMOkLib.CustomConfiguration;

public interface ITaskStartup
{
    TimeSpan Interval { get; }
    TimeSpan FirstRunDelay { get; }
}
