using System;
using System.Buffers;
using System.Text.Json;

namespace ConfigTest.JsonExtensions
{
	public static class JsonExtensions
	{
		public static JsonDocument ToJsonDocument<T>(this T value, JsonSerializerOptions options = null)
		{
			return JsonDocument.Parse(JsonSerializer.Serialize(value, options));
		}

		public static T ToObject<T>(this JsonElement element, JsonSerializerOptions options = null)
		{
			var bufferWriter = new ArrayBufferWriter<byte>();
			using (var writer = new Utf8JsonWriter(bufferWriter))
				element.WriteTo(writer);
			return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
		}

		public static T ToObject<T>(this JsonDocument document, JsonSerializerOptions options = null)
		{
			if (document == null)
				throw new ArgumentNullException(nameof(document));
			return document.RootElement.ToObject<T>(options);
		}
	}
}
