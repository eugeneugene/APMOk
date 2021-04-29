using System.Text.Json;

namespace ConfigTest.JsonDocumentExtensions
{
	/// <summary>
	/// Provides extension functionality for <see cref="JsonDocument"/>.
	/// </summary>
	public static class JsonDocumentExtensions
	{
		/// <summary>
		/// Converts an object to a <see cref="JsonDocument"/>.
		/// </summary>
		/// <typeparam name="T">The type of the object.</typeparam>
		/// <param name="value">The value to convert.</param>
		/// <param name="options">(optional) JSON serialization options.</param>
		/// <returns>A <see cref="JsonDocument"/> representing the vale.</returns>
		public static JsonDocument ToJsonDocument<T>(this T value, JsonSerializerOptions options = null)
		{
			return JsonDocument.Parse(JsonSerializer.Serialize(value, options));
		}
	}
}
