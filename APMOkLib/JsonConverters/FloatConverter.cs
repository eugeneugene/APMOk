using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APMOkLib.JsonConverters;

public class FloatConverter : JsonConverter<float>
{
    public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrEmpty(stringValue))
                return default;
            if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                return value;
        }
        else if (reader.TokenType == JsonTokenType.Number)
            return reader.GetSingle();

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
    {
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));
        writer.WriteNumberValue(value);
    }
}
