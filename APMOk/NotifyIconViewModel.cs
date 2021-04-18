using System.Windows;
using System.Windows.Input;

namespace APMOk
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public class NotifyIconViewModel
    {
        private DeviceStatusWindow deviceStatusWindow = null;

        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand => new DelegateCommand
        {
            CanExecuteFunc = () => deviceStatusWindow == null || !deviceStatusWindow.IsOpen(),
            CommandAction = () =>
            {
                deviceStatusWindow = new();
                deviceStatusWindow.Show();
            },
        };

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public ICommand HideWindowCommand => new DelegateCommand
        {
            CanExecuteFunc = () => deviceStatusWindow != null && deviceStatusWindow.IsOpen(),
            CommandAction = () =>
            {
                deviceStatusWindow?.Close();
                deviceStatusWindow = null;
            },
        };


        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public static ICommand ExitApplicationCommand => new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
    }
}
