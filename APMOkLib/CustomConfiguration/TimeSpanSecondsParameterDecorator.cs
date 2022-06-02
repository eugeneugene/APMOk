using Microsoft.Extensions.Configuration;
using System;

namespace APMOkLib.CustomConfiguration;

public class TimeSpanSecondsParameterDecorator : IParameterDecorator<TimeSpan>
{
    private readonly TimeSpan _defaultValue;

    public TimeSpan DefaultValue => _defaultValue;

    public TimeSpanSecondsParameterDecorator(TimeSpan DefaultValue)
    {
        _defaultValue = DefaultValue;
    }

    public TimeSpanSecondsParameterDecorator(double Seconds)
    {
        _defaultValue = TimeSpan.FromSeconds(Seconds);
    }

    public TimeSpanSecondsParameterDecorator(IConfigurationParameter<TimeSpan> timeSpanParameter)
    {
        _defaultValue = timeSpanParameter.Value;
    }

    public TimeSpan ExtractValue(IConfiguration configuration, string section)
    {
        if (configuration.GetSection(section).Exists())
        {
            var value = configuration.GetValue<double>(section);
            return TimeSpan.FromSeconds(value);
        }

        return _defaultValue;
    }
}
