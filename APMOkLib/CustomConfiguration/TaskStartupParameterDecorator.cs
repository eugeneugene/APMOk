using Microsoft.Extensions.Configuration;
using System;

namespace APMOkLib.CustomConfiguration
{
    public class TaskStartupParameterDecorator<T> : IParameterDecorator<T> where T : class, ITaskStartup
    {
        public T DefaultValue { get; }

        public TaskStartupParameterDecorator(T defaultValue)
        {
            DefaultValue = defaultValue ?? throw new ArgumentNullException(nameof(defaultValue));
        }

        public TaskStartupParameterDecorator(IConfigurationParameter<T> defaultValue)
        {
            if (defaultValue is null)
                throw new ArgumentNullException(nameof(defaultValue));

            DefaultValue = defaultValue.Value ?? throw new ArgumentException("Default Value cannot be null");
        }

        public T? ExtractValue(IConfiguration configuration, string section)
        {
            TimeSpan Interval = DefaultValue!.Interval;
            TimeSpan FirstRunDelay = DefaultValue!.FirstRunDelay;
            if (configuration.GetSection(section).Exists())
            {
                var val = configuration.GetValue(section, string.Empty);
                var arr = val.Split(',', ';', StringSplitOptions.TrimEntries);
                if (arr.Length >= 1 && int.TryParse(arr[0], out int res1))
                    Interval = TimeSpan.FromSeconds(res1);
                if (arr.Length >= 2 && int.TryParse(arr[1], out int res2))
                    FirstRunDelay = TimeSpan.FromSeconds(res2);
            }

            return new TaskStartupParameter(Interval, FirstRunDelay) as T;
        }
    }
}
