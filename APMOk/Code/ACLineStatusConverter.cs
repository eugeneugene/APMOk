using APMData.Proto;
using System;
using System.Globalization;
using System.Windows.Data;

namespace APMOk.Code
{
    internal class ACLineStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PowerStateReply reply)
            {
                return reply.ACLineStatus switch
                {
                    EACLineStatus.Online => 1,
                    EACLineStatus.Offline => 0,
                    EACLineStatus.LineStatusUnknown => -1,
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
