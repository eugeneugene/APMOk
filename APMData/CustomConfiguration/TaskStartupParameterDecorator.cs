using Microsoft.Extensions.Configuration;
using System;

namespace CustomConfiguration
{
    public class TaskStartupParameterDecorator<T> : IParameterDecorator<T> where T : class, ITaskStartup
    {
        private readonly T _defaultValue;

        public TaskStartupParameterDecorator(T defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public T ExtractValue(IConfiguration configuration, string section)
        {
            TimeSpan Interval = _defaultValue.Interval;
            TimeSpan FirstRunDelay = _defaultValue.FirstRunDelay;
            if (configuration.GetSection(section).Exists())
            {
                var val = configuration.GetValue(section, string.Empty);
                var arr = val.Split(',', ';', StringSplitOptions.TrimEntries);
                if (arr.Length >= 1 && int.TryParse(arr[0], out int res1))
                    Interval = TimeSpan.FromSeconds(res1);
                if (arr.Length >= 2 && int.TryParse(arr[1], out int res2))
                    FirstRunDelay = TimeSpan.FromSeconds(res2);
            }
            return new DefaultTaskStartup(Interval, FirstRunDelay) as T;
        }
    }
}
