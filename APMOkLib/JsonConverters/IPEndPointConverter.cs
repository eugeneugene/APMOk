using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APMOkLib.JsonConverters;

public class IPEndPointConverter : JsonConverter<IPEndPoint>
{
    public override IPEndPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        if (string.IsNullOrEmpty(stringValue))
            return default;
        if (IPEndPoint.TryParse(stringValue, out var result))
            return result;
        throw new JsonException("Can't deserialize IPEndPoint");
    }

    public override void Write(Utf8JsonWriter writer, IPEndPoint value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Address", value.Address.ToString());
        writer.WriteNumber("Port", value.Port);
        writer.WriteEndObject();
    }
}
