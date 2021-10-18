using System.Windows.Input;

namespace APMOk.Commands
{
    public static class APMOkCommands
    {
        public static RoutedCommand SetAPMValueCommand { get; } = new RoutedCommand("SetAPMValue", typeof(App));
        public static RoutedCommand SetAPMValueMenuCommand { get; } = new RoutedCommand("SetAPMValueMenu", typeof(App));
        public static RoutedCommand SetAPMCustomValueMenuCommand { get; } = new RoutedCommand("SetAPMCustomValueMenu", typeof(App));
    }
}
