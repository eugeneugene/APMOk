using APMOk.Code;
using APMOk.Models;
using APMOk.Services;
using APMOk.Tasks;
using APMOkLib;
using APMOkLib.CustomConfiguration;
using APMOkLib.RecurrentTasks;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Version = APMOk.Services.Version;

namespace APMOk
{
    public partial class Applic : Application, IDisposable
    {
        private IHost? host;
        private TaskbarIcon? notifyIcon;
        private bool disposedValue;

        [STAThread()]
        public static void Main()
        {
            EventWaitHandle? @event = null;
            try
            {
                @event = new(true, EventResetMode.AutoReset, "__APMOKAPP_INSTANCE", out bool created);
                if (!created)
                    return;

                Applic app = new();
                app.InitializeComponent();
                app.Run();
            }
            finally
            {
                @event?.Dispose();
            }
        }

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
                    services.AddSingleton<APMOkModel>();
                    services.AddSingleton<NotifyIconViewModel>();
                    services.AddTransient<ISocketPathProvider, SocketPathProvider>();
                    services.AddSingleton<IGrpcChannelProvider, GrpcChannelProvider>();
                    services.AddSingleton<DiskInfo>();
                    services.AddSingleton<PowerState>();
                    services.AddSingleton<APM>();
                    services.AddSingleton<Configuration>();
                    services.AddSingleton<Version>();
                    services.AddTransient<DeviceStatusWindow>();
                    services.AddHostedService<NotificationIconUpdater>();

                    services.AddSingleton<ConfigurationParameterFactory>();
                    services.AddSingleton<ITasksStartupConfiguration, TasksStartupConfiguration>();

                    services.AddTask<PowerStatusReaderTask>((options) =>
                    {
                        var service = options.ServiceProvider.GetRequiredService<ITasksStartupConfiguration>();
                            options.TaskOptions.AutoStart(service.PowerStatusReader.Value!);
                    });
                    services.AddTask<DiskInfoReaderTask>((options) =>
                    {
                        var service = options.ServiceProvider.GetRequiredService<ITasksStartupConfiguration>();
                            options.TaskOptions.AutoStart(service.DiskStatusReader.Value!);
                    });
                });
                host = hostBuilder.Build();
                await host.StartAsync();

                var _batteryStatusReaderTask = host.Services.GetRequiredService<ITask<PowerStatusReaderTask>>();
                _batteryStatusReaderTask.TryRunImmediately();

                var _diskInfoReaderTask = host.Services.GetRequiredService<ITask<DiskInfoReaderTask>>();
                _diskInfoReaderTask.TryRunImmediately();

                var _notifyIconViewModel = host.Services.GetRequiredService<NotifyIconViewModel>();
                notifyIcon.DataContext = _notifyIconViewModel;
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await host.StopAsync(TimeSpan.FromSeconds(5));
            host?.Dispose();
            host = null;
            base.OnExit(e);
        }

        private void AppDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Unhandled exception: {e.Exception}");
            if (e.Exception is COMException comException && comException.ErrorCode == -2147221040)
                e.Handled = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    notifyIcon?.Dispose();
                    host?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
