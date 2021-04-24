using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows.Markup;

namespace APMOk
{
    public partial class App : Application
    {
        private IHost host;
        private TaskbarIcon notifyIcon;

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<DeviceStatusWindow>();
            services.AddSingleton(notifyIcon);
        }

        protected override async void OnStartup(StartupEventArgs e)
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
                ConfigureServices(services);
            });
            host = hostBuilder.Build();
            notifyIcon.DataContext = new NotifyIconViewModel(host.Services);

            await host.StartAsync();

            base.OnStartup(e);
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
