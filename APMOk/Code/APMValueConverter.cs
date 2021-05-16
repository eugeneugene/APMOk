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
            if (val < 0)
                return "Error";
            if (val == 0)
                return "n/a";
            return val.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
