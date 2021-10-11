using System;
using System.Globalization;
using System.Windows.Data;

namespace APMOk.Code.Converters
{
    internal class APMValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                uint v when v == 0 => "n/a",
                uint v when v > 0 => value.ToString(),
                _ => throw new NotImplementedException(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
