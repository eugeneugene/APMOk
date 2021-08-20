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
                if (reply.ReplyResult == 0)
                    return -1;
                return reply.PowerState.PowerSource switch
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
