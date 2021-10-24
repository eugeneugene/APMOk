using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace APMOkLib
{
    /// <summary>
    /// Универсальный конвертер типов в строки
    /// </summary>
    public abstract class JsonToString : IFormattable
    {
        /// <summary>
        /// Перегруженный метод ToString
        /// </summary>
        /// <returns>Строка</returns>
        public override string ToString()
        {
            return ToString(string.Empty);
        }

        public virtual string ToString(string format)
        {
            return MakeString(this, format ?? string.Empty, null);
        }

        public static string MakeString(object ob)
        {
            return MakeString(ob, string.Empty, null);
        }

        public static string MakeString(object ob, string format, IFormatProvider? formatProvider)
        {
            if (format is null)
                throw new ArgumentNullException(nameof(format));

            if (ob is null)
                return "<null>";

            var options = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                IgnoreNullValues = false,
            };

            if (formatProvider is not null)
                options.Converters.Add(new StringJsonDateTimeConverter(formatProvider));

            if (format.Equals("e", StringComparison.Ordinal))
                options.Converters.Add(new JsonStringEnumConverter(new UpperCaseNamingPolicy()));
            if (format.Equals("E", StringComparison.Ordinal))
                options.Converters.Add(new JsonStringEnumStringNumConverter(new UpperCaseNamingPolicy()));

            var jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(ob, ob.GetType(), options);
            return Encoding.UTF8.GetString(jsonUtf8Bytes);
        }

        public virtual string ToString(string? format, IFormatProvider? formatProvider)
        {
            return MakeString(this, format ?? string.Empty, formatProvider);
        }
    }
}
