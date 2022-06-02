using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APMOkLib.JsonConverters;

public class IPAddressConverter : JsonConverter<IPAddress>
{
    public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        if (string.IsNullOrEmpty(stringValue))
            return default;
        if (IPAddress.TryParse(stringValue, out var result))
            return result;
        throw new JsonException("Can't deserialize IPAddress");
    }

    public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Address", value.ToString());
        writer.WriteEndObject();
    }
}
