using APMData;
using System;
using System.Globalization;
using System.Windows.Data;

namespace APMOk.Code.Converters
{
    internal class VersionConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return string.Empty;
            if (value is ServiceVersionReply versionReply)
            {
                var v = new Version(versionReply.Major, versionReply.Minor, versionReply.Build, versionReply.Revision);
                return $"{versionReply.ServiceName} {v}";
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException($"In {nameof(APMValueConverter)}");
        }
    }
}
