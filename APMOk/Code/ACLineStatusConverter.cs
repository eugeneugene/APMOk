using APMData.Proto;
using System;
using System.Globalization;
using System.Windows.Data;

namespace APMOk.Code
{
    internal class PowerSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PowerStateReply reply)
            {
                return reply.PowerSource switch
                {
                    EPowerSource.Battery => 0,
                    EPowerSource.Mains => 1,
                    EPowerSource.Unknown => -1,
                    _ => throw new NotImplementedException(),
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
