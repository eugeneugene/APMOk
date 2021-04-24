using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace APMOk
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
            return ToString("");
        }

        public virtual string ToString(string format)
        {
            return MakeString(this, format ?? "", null);
        }

        public static string MakeString(object ob)
        {
            return MakeString(ob, "", null);
        }

        public static string MakeString(object ob, string format, IFormatProvider formatProvider)
        {
            //if (ob == null)
            //    throw new ArgumentNullException(nameof(ob));
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            if (ob == null)
                return "<null>";

            var options = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                IgnoreNullValues = false,
            };
            if (formatProvider != null)
            {
                options.Converters.Add(new JsonToStringNullableDateTimeConverter(formatProvider));
                options.Converters.Add(new JsonToStringDateTimeConverter(formatProvider));
            }
            if (format.Contains("e"))
                options.Converters.Add(new JsonStringEnumConverter(new UpperCaseNamingPolicy()));
            var jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(ob, ob.GetType(), options);
            return Encoding.UTF8.GetString(jsonUtf8Bytes);
        }

        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            return MakeString(this, format ?? "", formatProvider);
        }
    }
}
