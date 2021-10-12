using System;

namespace APMOkLib
{
    public sealed class StringDateTimeConverter : DateTimeConverterBase
    {
        public static string FormatStatic => "yyyy-MM-dd\\THH:mm:ss.fff";

        public StringDateTimeConverter(DateTime dateTime) : base(dateTime)
        {
            FormatProvider = InvariantCulture;
        }

        public StringDateTimeConverter(DateTime dateTime, IFormatProvider formatProvider) : base(dateTime)
        {
            FormatProvider = formatProvider ?? InvariantCulture;
        }

        protected override string Format { get => FormatStatic; }
        protected override IFormatProvider FormatProvider { get; }
    }
}
