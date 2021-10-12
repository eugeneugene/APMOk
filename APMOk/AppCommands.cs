using System.Windows.Input;

namespace APMOk
{
    public sealed class AppCommands
    {
        public static RoutedCommand OnMainsCommand { get; } = new RoutedCommand(nameof(OnMainsCommand), typeof(App));
        public static RoutedCommand OnBatteriesCommand { get; } = new RoutedCommand(nameof(OnBatteriesCommand), typeof(App));

    }
}
