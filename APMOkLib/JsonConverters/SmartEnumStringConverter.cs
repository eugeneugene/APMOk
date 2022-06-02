using APMOkLib.SmartEnum;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APMOkLib.JsonConverters;

public class SmartEnumStringConverter<T> : JsonConverter<T> where T : SmartEnum<T, string>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (stringValue is not null)
            {
                if (SmartEnum<T, string>.TryFromValue(stringValue, out T? value1))
                    return value1!;
                if (SmartEnum<T, string>.TryFromValue(stringValue.ToUpperInvariant(), out T? value2))
                    return value2!;
            }
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
