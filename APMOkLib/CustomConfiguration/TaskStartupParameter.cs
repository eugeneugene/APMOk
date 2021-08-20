using System;
using System.Text.Json.Serialization;

namespace APMOkLib.CustomConfiguration
{

    public class TaskStartupParameter : JsonToString, ITaskStartup
    {
        [JsonConverter(typeof(TaskStartupParameterTimeSpanConverter))]
        public TimeSpan Interval { get; }

        [JsonConverter(typeof(TaskStartupParameterTimeSpanConverter))]
        public TimeSpan FirstRunDelay { get; }

        public TaskStartupParameter(TimeSpan interval, TimeSpan firstRunDelay)
        {
            Interval = interval;
            FirstRunDelay = firstRunDelay;
        }

        public TaskStartupParameter(TimeSpan interval)
        {
            Interval = interval;
            FirstRunDelay = TimeSpan.Zero;
        }

        public TaskStartupParameter(int intervalSec, int firstRunDelaySec)
        {
            Interval = TimeSpan.FromSeconds(intervalSec);
            FirstRunDelay = TimeSpan.FromSeconds(firstRunDelaySec);
        }

        public TaskStartupParameter(int intervalSec)
        {
            Interval = TimeSpan.FromSeconds(intervalSec);
            FirstRunDelay = TimeSpan.Zero;
        }
    }
}
