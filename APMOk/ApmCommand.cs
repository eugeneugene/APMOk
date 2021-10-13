using System.Windows.Input;

namespace APMOk
{
    public sealed class ApmCommand
    {
        public static RoutedCommand OnApmCommand { get; } = new RoutedCommand(nameof(OnApmCommand), typeof(App));
    }
}
