using System;
using System.Globalization;

namespace APMOkLib.JsonConverters.DateTimeConverters;

public class DateTimeConverter<T> where T : IDateTimeFormat, new()
{
    private readonly DateTime _dateTime;
    private readonly IFormatProvider _formatProvider;
    private readonly T _dateTimeFormat = new();

    public DateTimeConverter(DateTime dateTime)
    {
        _dateTime = dateTime;
        _formatProvider = CultureInfo.InvariantCulture;
    }

    public DateTimeConverter(DateTime dateTime, IFormatProvider formatProvider)
    {
        _dateTime = dateTime;
        _formatProvider = formatProvider ?? CultureInfo.InvariantCulture;
    }

    public string Convert()
    {
        return _dateTime.ToString(_dateTimeFormat.Format, _formatProvider);
    }

    public static implicit operator string(DateTimeConverter<T> converterBase) => converterBase.Convert();
}
