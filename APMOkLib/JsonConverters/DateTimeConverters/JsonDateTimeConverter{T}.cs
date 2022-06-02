using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APMOkLib.JsonConverters.DateTimeConverters;

public class JsonDateTimeConverter<T> : JsonConverter<DateTime> where T : IDateTimeFormat, new()
{
    private readonly T _dateTimeFormat = new();

    protected IFormatProvider FormatProvider { get; }

    public JsonDateTimeConverter()
    {
        FormatProvider = CultureInfo.InvariantCulture;
    }

    public JsonDateTimeConverter(IFormatProvider formatProvider)
    {
        FormatProvider = formatProvider ?? CultureInfo.InvariantCulture;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString();
        if (s is null)
            throw new JsonException("JSON token cannot be null");

        return DateTime.ParseExact(s, _dateTimeFormat.Format, FormatProvider, DateTimeStyles.AssumeLocal);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));
        writer.WriteStringValue(value.ToString(_dateTimeFormat.Format, FormatProvider));
    }
}
