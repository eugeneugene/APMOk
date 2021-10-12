using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APMOkLib
{
    public abstract class JsonNullableDateTimeConverterBase : JsonConverter<DateTime?>
    {
        protected static CultureInfo InvariantCulture { get => CultureInfo.InvariantCulture; }

        protected abstract string Format { get; }
        protected abstract IFormatProvider FormatProvider { get; }

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (DateTime.TryParseExact(reader.GetString(), Format, FormatProvider, DateTimeStyles.AssumeLocal, out DateTime result))
                return result;
            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (!value.HasValue)
                writer.WriteNullValue();
            writer.WriteStringValue(value.Value.ToString(Format, FormatProvider));
        }

        public string Convert(DateTime? dateTime) => dateTime?.ToString(Format, FormatProvider) ?? "null";
    }

    public sealed class JsonToStringNullableDateTimeConverter : JsonNullableDateTimeConverterBase
    {
        public JsonToStringNullableDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public JsonToStringNullableDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => StringDateTimeConverter.FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class IssNullableDateTimeConverter : JsonNullableDateTimeConverterBase
    {
        public IssNullableDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public IssNullableDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => IssDateTimeConverter.FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class CmiuJsonNullableDateTimeConverter : JsonNullableDateTimeConverterBase
    {
        public CmiuJsonNullableDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public CmiuJsonNullableDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => CmiuJsonDateTimeConverter.FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class SvcJsonNullableDateTimeConverter : JsonNullableDateTimeConverterBase
    {
        public SvcJsonNullableDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public SvcJsonNullableDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => SvcJsonDateTimeConverter.FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class AsopMetroJsonNullableDateTimeConverter : JsonNullableDateTimeConverterBase
    {
        public AsopMetroJsonNullableDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public AsopMetroJsonNullableDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => AsopMetroJsonDateTimeConverter.FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class EppNullableDateTimeConverter : JsonNullableDateTimeConverterBase
    {
        public EppNullableDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public EppNullableDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => AsopMetroJsonDateTimeConverter.FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class CameNullableDateTimeConverter : JsonNullableDateTimeConverterBase
    {
        public CameNullableDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public CameNullableDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => CameJsonDateTimeConverter.FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class JsonReportNullableDateTimeConverter : JsonNullableDateTimeConverterBase
    {
        public JsonReportNullableDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public JsonReportNullableDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => JsonReportDateTimeConverter.FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }

    public sealed class JsonUpdaterNullableDateTimeConverter : JsonNullableDateTimeConverterBase
    {
        public JsonUpdaterNullableDateTimeConverter()
        {
            FormatProvider = InvariantCulture;
        }

        public JsonUpdaterNullableDateTimeConverter(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => JsonReportDateTimeConverter.FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }
}
