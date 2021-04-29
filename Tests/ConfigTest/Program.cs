using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;

namespace ConfigTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();

            try
            {
                var builder = CreateHostBuilder(args);
                var host = builder.Build();
                host.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);
            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
            builder = builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
            });
            builder = builder.UseNLog();
            builder = builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddCommandLine(args);
            });

            builder = builder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
            return builder;
        }
    }
}
