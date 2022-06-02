using System;
using System.Globalization;

namespace APMOkLib.JsonConverters.DateTimeConverters;

public class DateTimeNullableConverter<T> where T : IDateTimeFormat, new()
{
    private readonly DateTime? _dateTime;
    private readonly IFormatProvider _formatProvider;
    private readonly T _dateTimeFormat = new();

    public DateTimeNullableConverter(DateTime? dateTime)
    {
        _dateTime = dateTime;
        _formatProvider = CultureInfo.InvariantCulture;
    }

    public DateTimeNullableConverter(DateTime? dateTime, IFormatProvider formatProvider)
    {
        _dateTime = dateTime;
        _formatProvider = formatProvider ?? CultureInfo.InvariantCulture;
    }

    public string? Convert()
    {
        return _dateTime?.ToString(_dateTimeFormat.Format, _formatProvider);
    }

    public static implicit operator string?(DateTimeNullableConverter<T> converterBase) => converterBase.Convert();
}
