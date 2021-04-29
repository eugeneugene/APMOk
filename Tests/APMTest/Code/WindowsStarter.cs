using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace APMTest.Code
{
    public class WindowsStarter<TStartup> : IStarter where TStartup : class
    {
        private readonly Logger logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
        private readonly List<string> HostArgs = new() { };

        PlatformID IStarter.Platform => PlatformID.Win32NT;

        public StarterArgumensResult ProcessCommandArgumens(ICollection<string> args) => StarterArgumensResult.Run;

        public async Task<StarterRunResult> ProcessHostRunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var hostbuilder = CreateHostBuilder(HostArgs.ToArray());
                await hostbuilder.RunConsoleAsync(cancellationToken);
                return StarterRunResult.Success;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return StarterRunResult.Error;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostbuilder = Host.CreateDefaultBuilder(args);

            hostbuilder = hostbuilder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            });
            hostbuilder = hostbuilder.UseNLog();

            hostbuilder.ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddCommandLine(args);
            });

            logger.Info("Running {0}", Environment.OSVersion.VersionString);
            hostbuilder = hostbuilder.UseWindowsService();
            logger.Info("Using Windows Service");

            hostbuilder = hostbuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TStartup>();
            });

            return hostbuilder;
        }

        void IStarter.ShowHelp()
        {
            Console.WriteLine("APMTest");
        }
    }
}
