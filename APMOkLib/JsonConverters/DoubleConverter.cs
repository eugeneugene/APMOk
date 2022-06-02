using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APMOkLib.JsonConverters;

public class DoubleConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrEmpty(stringValue))
                return default;
            if (double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                return value;
        }
        else if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDouble();

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));
        writer.WriteNumberValue(value);
    }
}
