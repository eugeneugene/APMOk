using System;

namespace CustomConfiguration
{
    public class DefaultTaskStartup : ITaskStartup
    {
        public TimeSpan Interval { get; }
        public TimeSpan FirstRunDelay { get; }

        public DefaultTaskStartup(TimeSpan interval, TimeSpan firstRunDelay)
        {
            Interval = interval;
            FirstRunDelay = firstRunDelay;
        }

        public DefaultTaskStartup(TimeSpan interval)
        {
            Interval = interval;
            FirstRunDelay = TimeSpan.FromSeconds(0);
        }

        public DefaultTaskStartup(int intervalSec, int firstRunDelaySec)
        {
            Interval = TimeSpan.FromSeconds(intervalSec);
            FirstRunDelay = TimeSpan.FromSeconds(firstRunDelaySec);
        }

        public DefaultTaskStartup(int intervalSec)
        {
            Interval = TimeSpan.FromSeconds(intervalSec);
            FirstRunDelay = TimeSpan.FromSeconds(0);
        }
    }
}
