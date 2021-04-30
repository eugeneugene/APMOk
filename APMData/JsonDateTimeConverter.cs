using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APMData
{
    public abstract class JsonDateTimeConverterBase : JsonConverter<DateTime>
    {
        protected static CultureInfo InvariantCulture { get => CultureInfo.InvariantCulture; }

        protected abstract string Format { get; }
        protected abstract IFormatProvider FormatProvider { get; }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.ParseExact(reader.GetString(), Format, FormatProvider, DateTimeStyles.AssumeLocal);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue(value.ToString(Format, FormatProvider));
        }

        public string Convert(DateTime dateTime) => dateTime.ToString(Format, FormatProvider);
    }

    public sealed class JsonToStringDateTimeConverter : JsonDateTimeConverterBase
    {
        public static string FormatStatic => "yyyy-MM-dd\\THH:mm:ss.fff";

        public JsonToStringDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public JsonToStringDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class IssDateTimeConverter : JsonDateTimeConverterBase
    {
        public static string FormatStatic => "dd-MM-yyyy HH:mm:ss.fff";

        public IssDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public IssDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class CmiuJsonDateTimeConverter : JsonDateTimeConverterBase
    {
        public static string FormatStatic => "dd-MM-yyyy HH:mm:ss";

        public CmiuJsonDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public CmiuJsonDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class SvcJsonDateTimeConverter : JsonDateTimeConverterBase
    {
        public static string FormatStatic => "yyyy-MM-dd\\THH:mm:ss.fff";

        public SvcJsonDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public SvcJsonDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class AsopMetroJsonDateTimeConverter : JsonDateTimeConverterBase
    {
        public static string FormatStatic => "yyyy-MM-dd\\THH:mm:sszzz";

        public AsopMetroJsonDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public AsopMetroJsonDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class EppDateTimeConverter : JsonDateTimeConverterBase
    {
        public static string FormatStatic => "yyyy-MM-dd\\THH:mm:sszzz";

        public EppDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public EppDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class CameJsonDateTimeConverter : JsonDateTimeConverterBase
    {
        public static string FormatStatic => "yyyyMMddHHmmss";

        public CameJsonDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public CameJsonDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class JsonReportDateTimeConverter : JsonDateTimeConverterBase
    {
        public static string FormatStatic => "dd.MM.yyyy HH:mm:ss.fff";

        public JsonReportDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public JsonReportDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class JsonUpdaterDateTimeConverter : JsonDateTimeConverterBase
    {
        public static string FormatStatic => "dd.MM.yyyy\\THH:mm:ss";

        public JsonUpdaterDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public JsonUpdaterDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }
}
