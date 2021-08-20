using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APMOkLib
{
    public class TaskStartupParameterTimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string enumValue = reader.GetString();
            if (TimeSpan.TryParse(enumValue, out TimeSpan result))
                return result;

            throw new JsonException($"Argument '{enumValue}' is not of type 'TimeSpan'");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("c"));
        }
    }
}
