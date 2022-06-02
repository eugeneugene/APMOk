using APMOk.Code;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Input;

namespace APMOk.Models;

/// <summary>
/// Provides bindable properties and commands for the NotifyIcon. In this sample, the
/// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
/// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
/// </summary>
internal class NotifyIconViewModel
{
    private readonly IServiceProvider _services;
    private DeviceStatusWindow? deviceStatusWindow;

    public NotifyIconViewModel(IServiceProvider services)
    {
        _services = services;
    }

    /// <summary>
    /// Shows a window, if none is already open.
    /// </summary>
    public ICommand ShowWindowCommand => new DelegateCommand
    {
        CommandAction = () =>
        {
            if (deviceStatusWindow is null || deviceStatusWindow.IsClosed())
                deviceStatusWindow = _services.GetRequiredService<DeviceStatusWindow>();

            if (deviceStatusWindow is not null)
            {
                if (!deviceStatusWindow.IsVisible)
                    deviceStatusWindow.Show();

                if (deviceStatusWindow.WindowState == WindowState.Minimized)
                    deviceStatusWindow.WindowState = WindowState.Normal;

                deviceStatusWindow.Activate();
                deviceStatusWindow.Topmost = true;  // important
                deviceStatusWindow.Topmost = false; // important
                deviceStatusWindow.Focus();         // important
            }
        }
    };

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    public static ICommand ExitApplicationCommand => new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
}
