using System;
using System.Globalization;

namespace APMOkLib
{
    public abstract class DateTimeConverterBase
    {
        private readonly DateTime _dateTime;
        public DateTimeConverterBase(DateTime dateTime)
        {
            _dateTime = dateTime;
        }

        protected static CultureInfo InvariantCulture { get => CultureInfo.InvariantCulture; }

        protected abstract string Format { get; }
        protected abstract IFormatProvider FormatProvider { get; }

        public static implicit operator string(DateTimeConverterBase converterBase) => converterBase._dateTime.ToString(converterBase.Format, converterBase.FormatProvider);
    }
}
