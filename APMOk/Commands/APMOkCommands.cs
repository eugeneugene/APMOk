using System.Windows.Input;

namespace APMOk.Commands
{
    public static class APMOkCommands
    {
        public static RoutedCommand SetAPMValueCommand { get; } = new RoutedCommand(nameof(SetAPMValueCommand), typeof(App));
        public static RoutedCommand SetAPMValueMenuCommand { get; } = new RoutedCommand(nameof(SetAPMValueMenuCommand), typeof(App));
        public static RoutedCommand SetAPMCustomValueMenuCommand { get; } = new RoutedCommand(nameof(SetAPMCustomValueMenuCommand), typeof(App));
        public static RoutedCommand ButtonOkCommand { get; } = new RoutedCommand(nameof(ButtonOkCommand), typeof(App));
    }
}
