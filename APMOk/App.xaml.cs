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
using System.Windows;
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

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await host.StopAsync(TimeSpan.FromSeconds(5));
            host.Dispose();

            notifyIcon.Dispose();

            base.OnExit(e);
        }
    }
}
