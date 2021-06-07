using System;
using System.Globalization;
using System.Windows.Data;

namespace APMOk.Code
{
    internal class APMValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val = (int)value;
            return val switch
            {
                int v when v < 0 => "Error",
                int v when v == 0 => "n/a",
                int v when v > 0 => val.ToString(CultureInfo.InvariantCulture),
                _ => throw new NotImplementedException(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
