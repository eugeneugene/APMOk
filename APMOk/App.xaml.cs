﻿using APMData;
using APMOk.Code;
using APMOk.Services;
using APMOk.Tasks;
using CustomConfiguration;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RecurrentTasks;
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
            var _apmOkData = host.Services.GetRequiredService<APMOkModel>();
            if (e.Source is TaskbarIcon taskbarIcon)
            {
                var items = taskbarIcon.ContextMenu.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (_apmOkData.SystemDiskInfo != null)
                    {
                        if (item is Separator)
                        {
                            int j = 0;
                            foreach (var diskInfoEntry in _apmOkData.SystemDiskInfo.DiskInfoEntries.Where(item => item.InfoValid).OrderBy(item => item.Index))
                            {
                                _apmOkData.APMValueDictionary.TryGetValue(diskInfoEntry.DeviceID, out var apmValueProperty);
                                var newMenuItem1 = new MenuItem { Name = $"ID{j}", Header = $"{diskInfoEntry.Index}. {diskInfoEntry.Caption}", };
                                if (apmValueProperty == null || apmValueProperty.DefaultValue == 0)
                                    newMenuItem1.Items.Add(new MenuItem { Header = "APM not available", IsEnabled = false });
                                else
                                {
                                    newMenuItem1.Items.Add(new MenuItem { Header = "Default value: " + apmValueProperty.DefaultValue });
                                    newMenuItem1.Items.Add(new MenuItem { Header = "User value: " + ((apmValueProperty.UserValue == 0) ? "n/a" : apmValueProperty.UserValue.ToString()) });
                                    newMenuItem1.Items.Add(new Separator());
                                    var newMenuItem2 = new MenuItem { Header = "Set" };
                                    newMenuItem2.Items.AddRange(GetSetMenuItems());
                                    newMenuItem1.Items.Add(newMenuItem2);
                                }

                                items.Insert(i + j + 1, newMenuItem1);
                                j++;
                            }
                            i += j;
                            continue;
                        }
                    }
                    if (item is MenuItem menuItem && menuItem.Name.StartsWith("ID"))
                    {
                        items.RemoveAt(i);
                        --i;
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
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
