using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APMOkLib.JsonConverters;

/// <summary>
/// Allow both string and number values on deserialize.
/// </summary>
public class Int32Converter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrEmpty(stringValue))
                return default;
            if (int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                return value;
        }
        else if (reader.TokenType == JsonTokenType.Number)
            return reader.GetInt32();

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));
        writer.WriteNumberValue(value);
    }
}
