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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace APMOk
{
    public partial class App : Application, IDisposable
    {
        private IHost host = null;
        private TaskbarIcon notifyIcon = null;
        private bool disposedValue = false;

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
                    services.AddTransient<DiskInfoService>();
                    services.AddTransient<PowerStateService>();
                    services.AddTransient<ConfigurationService>();
                    services.AddTransient<DeviceStatusWindow>();
                    services.AddHostedService<NotificationIconUpdaterTask>();

                    services.AddSingleton<ConfigurationParameterFactory>();
                    services.AddSingleton<ITasksStartupConfiguration, TasksStartupConfiguration>();

                    services.AddTask<PowerStatusReaderTask>(options => options.TaskOptions.AutoStart(options.ServiceProvider.GetRequiredService<ITasksStartupConfiguration>().PowerStatusReader.Value));
                    services.AddTask<DiskInfoReaderTask>(options => options.TaskOptions.AutoStart(options.ServiceProvider.GetRequiredService<ITasksStartupConfiguration>().DiskStatusReader.Value));
                });
                host = hostBuilder.Build();
                await host.StartAsync();

                var _batteryStatusReaderTask = host.Services.GetRequiredService<ITask<PowerStatusReaderTask>>();
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
            var _apmOkData = host.Services.GetRequiredService<APMOkModel>();
            if (e.Source is TaskbarIcon taskbarIcon)
            {
                var items = taskbarIcon.ContextMenu.Items;
                foreach (var item in items)
                {
                    if (item is MenuItem menuItem1 && menuItem1.Name == "OnMains")
                    {
                        while (menuItem1.Items.Count > 0)
                            menuItem1.Items.RemoveAt(0);

                        if (!_apmOkData.ConnectFailure)
                        {
                            menuItem1.IsEnabled = true;
                            int j = 0;
                            foreach (var diskInfoEntry in _apmOkData.SystemDiskInfo.DiskInfoEntries.Where(item => item.InfoValid).OrderBy(item => item.Index))
                            {
                                var newMenuItem = new MenuItem { Name = $"ID{j++}", Header = $"{diskInfoEntry.Index}. {diskInfoEntry.Caption}", };
                                _apmOkData.APMValueDictionary.TryGetValue(diskInfoEntry.DeviceID, out var apmValueProperty);
                                if (apmValueProperty == null)
                                    newMenuItem.Items.Add(new MenuItem { Header = "APM not available", IsEnabled = false });
                                else
                                {
                                    newMenuItem.Items.Add(new MenuItem { Header = "On Mains: " + ((apmValueProperty.OnMains == 0) ? "n/a" : apmValueProperty.OnMains.ToString()) });
                                    newMenuItem.Items.Add(new Separator());
                                    var newMenuItem2 = new MenuItem { Header = "Set" };
                                    newMenuItem2.Items.AddRange(GetSetMenuItems());
                                    newMenuItem.Items.Add(newMenuItem2);
                                }

                                menuItem1.Items.Add(newMenuItem);
                            }
                        }
                        else
                            menuItem1.IsEnabled = false;
                    }
                    if (item is MenuItem menuItem2 && menuItem2.Name == "OnBattery")
                    {
                        while (menuItem2.Items.Count > 0)
                            menuItem2.Items.RemoveAt(0);

                        if (!_apmOkData.ConnectFailure)
                        {
                            menuItem2.IsEnabled = true;
                            int j = 0;
                            foreach (var diskInfoEntry in _apmOkData.SystemDiskInfo.DiskInfoEntries.Where(item => item.InfoValid).OrderBy(item => item.Index))
                            {
                                var newMenuItem = new MenuItem { Name = $"ID{j++}", Header = $"{diskInfoEntry.Index}. {diskInfoEntry.Caption}", };
                                _apmOkData.APMValueDictionary.TryGetValue(diskInfoEntry.DeviceID, out var apmValueProperty);
                                if (apmValueProperty == null)
                                    newMenuItem.Items.Add(new MenuItem { Header = "APM not available", IsEnabled = false });
                                else
                                {
                                    newMenuItem.Items.Add(new MenuItem { Header = "On Batteries: " + ((apmValueProperty.OnBatteries == 0) ? "n/a" : apmValueProperty.OnBatteries.ToString()) });
                                    newMenuItem.Items.Add(new Separator());
                                    var newMenuItem2 = new MenuItem { Header = "Set" };
                                    newMenuItem2.Items.AddRange(GetSetMenuItems());
                                    newMenuItem.Items.Add(newMenuItem2);
                                }

                                menuItem2.Items.Add(newMenuItem);
                            }
                        }
                        else
                            menuItem2.IsEnabled = false;
                    }
                }
            }
        }

        private static IEnumerable<Control> GetSetMenuItems()
        {
            List<Control> items = new();

            foreach (APMLevel level in Enum.GetValues(typeof(APMLevel)))
            {
                if (!level.NotMapped())
                    items.Add(new MenuItem { Header = level.DisplayEnum() });
            }
            items.Add(new Separator());
            items.Add(new MenuItem { Header = "Set custom value" });

            return items;
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await host.StopAsync(TimeSpan.FromSeconds(5));
            host.Dispose();
            host = null;
            base.OnExit(e);
        }

        private void AppDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("Unhandled exception: {0}", e.Exception);
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
