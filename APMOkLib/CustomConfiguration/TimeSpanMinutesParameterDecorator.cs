using Microsoft.Extensions.Configuration;
using System;

namespace APMOkLib.CustomConfiguration
{
    public class TimeSpanMinutesParameterDecorator : IParameterDecorator<TimeSpan>
    {
        private readonly TimeSpan _defaultValue;

        public TimeSpan DefaultValue => _defaultValue;

        public TimeSpanMinutesParameterDecorator(TimeSpan DefaultValue)
        {
            _defaultValue = DefaultValue;
        }

        public TimeSpanMinutesParameterDecorator(double Minutes)
        {
            _defaultValue = TimeSpan.FromMinutes(Minutes);
        }

        public TimeSpanMinutesParameterDecorator(IConfigurationParameter<TimeSpan> timeSpanParameter)
        {
            _defaultValue = timeSpanParameter.Value;
        }

        public TimeSpan ExtractValue(IConfiguration configuration, string section)
        {
            if (configuration.GetSection(section).Exists())
            {
                var value = configuration.GetValue<double>(section);
                return TimeSpan.FromMinutes(value);
            }

            return _defaultValue;
        }
    }
}
