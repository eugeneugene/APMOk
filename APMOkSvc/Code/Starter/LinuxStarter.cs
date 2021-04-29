using APMData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace APMOkSvc.Code
{
    public class LinuxStarter<TStartup> : IStarter where TStartup : class
    {
        private bool console;
        private bool daemon;

        private readonly Logger logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
        private readonly List<string> HostArgs = new() { };
        private readonly string location;
        private readonly string file;

        public PlatformID Platform => PlatformID.Unix;

        public LinuxStarter()
        {
            var ass = Assembly.GetExecutingAssembly();
            location = ass.Location;
            if (location.Contains(" ", StringComparison.InvariantCulture))
                location = $"\"{location}\"";
            file = Path.GetFileName(ass.Location);
        }

        public StarterArgumensResult ProcessCommandArgumens(ICollection<string> args)
        {
            if (args == null)
            {
                Console.Error.WriteLine("Набор аргументов равен null");
                return StarterArgumensResult.ExitError;
            }

            foreach (var arg in args)
            {
                switch (arg.ToUpperInvariant())
                {
                    case "-H":
                    case "/H":
                    case "-?":
                    case "/?":
                    case "-HELP":
                    case "/HELP":
                    case "--HELP":
                        return StarterArgumensResult.HelpNoError;
                    case "-C":
                    case "/C":
                    case "-CONSOLE":
                    case "/CONSOLE":
                    case "--CONSOLE":
                        console = true;
                        break;
                    case "-D":
                    case "/D":
                    case "-DAEMON":
                    case "/DAEMON":
                    case "--DAEMON":
                        daemon = true;
                        break;
                    case "MOO":
                    case "/MOO":
                    case "-MOO":
                    case "--MOO":
                        MooL();
                        return StarterArgumensResult.ExitNoError;
                    default:
                        HostArgs.Add(arg);
                        break;
                }
            }

            if (console && daemon)
            {
                Console.Error.WriteLine("Использованы взаимоисключающие опции");
                return StarterArgumensResult.HelpError;
            }

            if (daemon)
                return StarterArgumensResult.Run;
            if (console)
                return StarterArgumensResult.Run;

            return StarterArgumensResult.HelpNoError;
        }

        public async Task<StarterRunResult> ProcessHostRunAsync(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var line in VersionHelper.VersionLines)
                    logger.Info(line);

                var hostbuilder = CreateHostBuilder(HostArgs.ToArray());
                if (daemon)
                {
                    logger.Info("Daemon execution");
                    await hostbuilder.Build().RunAsync(cancellationToken);
                    return StarterRunResult.Success;
                }
                if (Debugger.IsAttached || console)
                {
                    logger.Info("Console execution");
                    await hostbuilder.RunConsoleAsync(cancellationToken);
                    return StarterRunResult.Success;
                }

                Console.WriteLine($"Попробуйте `{file} --help' для справки");
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

        public void ShowHelp()
        {
            Console.WriteLine(VersionHelper.Version);
            Console.WriteLine("Использование:\t{0} <arg>", file);
            Console.WriteLine("Где <arg> - одно из:");
            Console.WriteLine("-h, -?, --help                    Показать это сообщение и выйти");
            Console.WriteLine("-d, --daemon                      Запустить, как службу");
            Console.WriteLine("-c, --console                     Запустить, как консольное приложение");
        }

        private static void MooL()
        {
            Console.Write(@"                 (__)
                 (oo)
           /L-----\/
          / |    ||
         *  /\---/\
            ~~   ~~
...""Have you mooed today ? ""...");
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

            hostbuilder = hostbuilder.UseSystemd();
            logger.Info("Using Linux Systemd Service");

            if (File.Exists(SocketData.SocketPath))
                File.Delete(SocketData.SocketPath);

            hostbuilder = hostbuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TStartup>();
                webBuilder.ConfigureKestrel(options =>
                {
                    options.ListenUnixSocket(SocketData.SocketPath, options =>
                    {
                        options.Protocols = HttpProtocols.Http1;
                    });
                });
            });

            return hostbuilder;
        }
    }
}
