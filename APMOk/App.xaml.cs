using APMOk.Services;
using APMOk.Tasks;
using CustomConfiguration;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RecurrentTasks;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace APMOk
{
    public partial class App : Application
    {
        private IHost host;
        private TaskbarIcon notifyIcon;

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentUICulture;
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;

                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

                notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

                var args = Environment.GetCommandLineArgs();
                var hostBuilder = Host.CreateDefaultBuilder(args);
                hostBuilder.ConfigureAppConfiguration((context, builder) => { });
                hostBuilder.ConfigureServices((context, services) =>
                {
                    services.AddSingleton(notifyIcon);
                    services.AddSingleton<APMOkData>();
                    services.AddSingleton<NotifyIconViewModel>();
                    services.AddTransient<DiskInfoService>();
                    services.AddTransient<PowerStateService>();
                    services.AddTransient<ConfigurationService>();
                    services.AddTransient<DeviceStatusWindow>();
                    services.AddHostedService<NotificationIconUpdaterTask>();

                    TasksStartupConfiguration tasksStartupConfiguration = new(context.Configuration);
                    services.AddTask<BatteryStatusReaderTask>(task => task.AutoStart(tasksStartupConfiguration.BatteryStatusReader.Value));
                    services.AddTask<DiskInfoReaderTask>(task => task.AutoStart(tasksStartupConfiguration.DiskStatusReader.Value));
                });
                host = hostBuilder.Build();
                await host.StartAsync();

                var _batteryStatusReaderTask = host.Services.GetRequiredService<ITask<BatteryStatusReaderTask>>();
                _batteryStatusReaderTask.TryRunImmediately();

                var _diskInfoReaderTask = host.Services.GetRequiredService<ITask<DiskInfoReaderTask>>();
                _diskInfoReaderTask.TryRunImmediately();

                var _notifyIconViewModel = host.Services.GetRequiredService<NotifyIconViewModel>();
                notifyIcon.DataContext = _notifyIconViewModel;
                notifyIcon.TrayContextMenuOpen += NotifyIconTrayContextMenuOpen;
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        private void NotifyIconTrayContextMenuOpen(object sender, RoutedEventArgs e)
        {
            var _apmOkData = host.Services.GetRequiredService<APMOkData>();
            if (e.Source is TaskbarIcon taskbarIcon)
            {
                var items = taskbarIcon.ContextMenu.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item is Separator)
                    {
                        int j = 0;
                        foreach (var diskInfoEntry in _apmOkData.SystemDiskInfo.DiskInfoEntries.Where(item => item.InfoValid).OrderBy(item => item.Index))
                        {
                            items.Insert(i + j + 1, new MenuItem { Name = $"ID{j}", Header = $"{diskInfoEntry.Index}. {diskInfoEntry.Caption}" });
                            j++;
                        }
                        i += j;
                        continue;
                    }
                    if (item is MenuItem menuItem && menuItem.Name.StartsWith("ID"))
                    {
                        items.RemoveAt(i);
                        --i;
                    }
                }
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await host.StopAsync(TimeSpan.FromSeconds(5));
            host.Dispose();

            notifyIcon.Dispose();

            base.OnExit(e);
        }

        private void AppDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("Unhandled exception: {0}", e.Exception);
            if (e.Exception is COMException comException && comException.ErrorCode == -2147221040)
                e.Handled = true;
        }
    }
}
