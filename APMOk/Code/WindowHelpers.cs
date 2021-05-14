using System.Linq;
using System.Windows;

namespace APMOk
{
    public static class WindowHelpers
    {
        public static bool IsClosed(this Window window)
        {
            return !Application.Current.Windows.Cast<Window>().Any(x => x == window);
        }
    }
}
