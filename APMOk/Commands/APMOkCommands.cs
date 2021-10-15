using System.Windows.Input;

namespace APMOk.Commands
{
    public static class APMOkCommands
    {
        public static RoutedCommand SetAPMValueCommand { get; } = new RoutedCommand("SetAPMValue", typeof(App));
    }
}
