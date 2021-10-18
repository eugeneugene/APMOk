using APMOkLib;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
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
    public class WindowsStarter<TStartup> : IStarter where TStartup : class
    {
        private bool console;
        private bool install;
        private bool uninstall;
        private bool start;
        private bool stop;
        private bool restart;
        private bool status;
        private bool debug;

        private readonly string servicename = Properties.Resources.ServiceName;
        private readonly string description = Properties.Resources.Description;
        private readonly string longDescription = Properties.Resources.LongDescription;

        private readonly TimeSpan ServiceRestartDelay = TimeSpan.FromSeconds(5);
        private readonly Logger logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
        private readonly List<string> HostArgs = new() { };
        private readonly string location;
        private readonly string file;

        public PlatformID Platform => PlatformID.Win32NT;

        public WindowsStarter()
        {
            var ass = Assembly.GetExecutingAssembly();
            location = Path.ChangeExtension(ass.Location, "exe");
            if (location.Contains(" ", StringComparison.InvariantCulture))
                location = $"\"{location}\"";
            file = Path.GetFileName(ass.Location);
        }

        public StarterArgumensResult ProcessCommandArgumens(ICollection<string> args)
        {
            if (args == null)
            {
                Console.Error.WriteLine("Arguments are null");
                return StarterArgumensResult.ExitError;
            }

            bool bExpectInstallArg = false;
            string sInstallArg = string.Empty;

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
                        bExpectInstallArg = false;
                        break;
                    case "-D":
                    case "/D":
                    case "-G":
                    case "/G":
                    case "-DEBUG":
                    case "/DEBUG":
                    case "--DEBUG":
                        debug = true;
                        bExpectInstallArg = false;
                        break;
                    case "-I":
                    case "/I":
                    case "-INSTALL":
                    case "/INSTALL":
                    case "--INSTALL":
                        install = true;
                        bExpectInstallArg = true;
                        break;
                    case "-U":
                    case "/U":
                    case "-UNINSTALL":
                    case "/UNINSTALL":
                    case "--UNINSTALL":
                        uninstall = true;
                        bExpectInstallArg = false;
                        break;
                    case "-S":
                    case "/S":
                    case "-START":
                    case "/START":
                    case "--START":
                        start = true;
                        bExpectInstallArg = false;
                        break;
                    case "-K":
                    case "/K":
                    case "-KILL":
                    case "/KILL":
                    case "--KILL":
                    case "-STOP":
                    case "--STOP":
                        stop = true;
                        bExpectInstallArg = false;
                        break;
                    case "-R":
                    case "/R":
                    case "-RESTART":
                    case "/RESTART":
                    case "--RESTART":
                        restart = true;
                        bExpectInstallArg = false;
                        break;
                    case "-T":
                    case "/T":
                    case "-STATUS":
                    case "/STATUS":
                    case "--STATUS":
                        status = true;
                        bExpectInstallArg = false;
                        break;
                    case "MOO":
                    case "/MOO":
                    case "-MOO":
                    case "--MOO":
                        MooW();
                        return StarterArgumensResult.ExitNoError;
                    default:
                        if (bExpectInstallArg)
                        {
                            sInstallArg = arg;
                            bExpectInstallArg = false;
                        }
                        else
                            HostArgs.Add(arg);
                        break;
                }
            }

            int i = 0;
            if (console) 
                ++i;
            if (install)                
                ++i;
            if (uninstall)
                ++i;
            if (start)
                ++i;
            if (stop)
                ++i;
            if (restart)
                ++i;
            if (status) 
                ++i;
            if (i > 1)
            {
                Console.Error.WriteLine("Used mutual exclusive options");
                return StarterArgumensResult.HelpError;
            }

            if (install)
            {
                ServiceInstallOptions installOptions = ServiceInstallOptions.Default;

                if (!string.IsNullOrEmpty(sInstallArg) && !Enum.TryParse(sInstallArg, out installOptions))
                {
                    Console.Error.WriteLine("Wrong argument '{0}'", sInstallArg);
                    return StarterArgumensResult.HelpError;
                }

                if (ServiceInstaller.ServiceIsInstalled(servicename))
                {
                    Console.Error.WriteLine("Service '{0}' is already installed", servicename);
                    return StarterArgumensResult.ExitError;
                }

                ServiceInstaller.Install(servicename, description, location, (ServiceBootFlag)installOptions);
                ServiceInstaller.SetServiceDescription(servicename, longDescription);
                SC_ACTION sc1 = new()
                {
                    Type = ScActionType.SC_ACTION_RESTART,
                    Delay = 1 * 60 * 000
                };
                SC_ACTION sc2 = new()
                {
                    Type = ScActionType.SC_ACTION_RESTART,
                    Delay = 5 * 60 * 1000
                };
                SC_ACTION sc3 = new()
                {
                    Type = ScActionType.SC_ACTION_RESTART,
                    Delay = 15 * 60 * 1000
                };
                ServiceInstaller.SetRecoveryOptions(servicename, sc1, sc2, sc3, 86400);
                Console.WriteLine("Service successfully installed");
                return StarterArgumensResult.ExitNoError;
            }
            if (uninstall)
            {
                if (ServiceInstaller.ServiceIsInstalled(servicename))
                    ServiceInstaller.Uninstall(servicename);
                else
                {
                    Console.Error.WriteLine("Service '{0}' is not installed", servicename);
                    return StarterArgumensResult.ExitError;
                }
                Console.WriteLine("Service successfully uninstalled");
                return StarterArgumensResult.ExitNoError;
            }
            if (start)
            {
                if (!ServiceInstaller.ServiceIsInstalled(servicename))
                {
                    Console.Error.WriteLine("Service '{0}' is not installed", servicename);
                    return StarterArgumensResult.ExitError;
                }
                ServiceInstaller.StartService(servicename);
                Console.WriteLine("Service started");
                return StarterArgumensResult.ExitNoError;
            }
            if (stop)
            {
                if (!ServiceInstaller.ServiceIsInstalled(servicename))
                {
                    Console.Error.WriteLine("Service '{0}' is not installed", servicename);
                    return StarterArgumensResult.ExitError;
                }
                ServiceInstaller.StopService(servicename);
                Console.WriteLine("Service stopped");
                return StarterArgumensResult.ExitNoError;
            }
            if (restart)
            {
                if (!ServiceInstaller.ServiceIsInstalled(servicename))
                {
                    Console.Error.WriteLine("Service '{0}' is not installed", servicename);
                    return StarterArgumensResult.ExitError;
                }
                ServiceInstaller.StopService(servicename);
                Console.WriteLine("Service stopped");
                Thread.Sleep(ServiceRestartDelay);
                ServiceInstaller.StartService(servicename);
                Console.WriteLine("Service started");
                return StarterArgumensResult.ExitNoError;
            }
            if (status)
            {
                if (!ServiceInstaller.ServiceIsInstalled(servicename))
                {
                    Console.Error.WriteLine("Service '{0}' is not installed", servicename);
                    return StarterArgumensResult.ExitError;
                }
                var state = ServiceInstaller.GetServiceStatus(servicename);
                string descr = state switch
                {
                    ServiceState.Stopped => "stopped",
                    ServiceState.NotFound => "not found",
                    ServiceState.PausePending => "in pause pending",
                    ServiceState.Paused => "paused",
                    ServiceState.Running => "running",
                    ServiceState.StartPending => "in start pending",
                    ServiceState.StopPending => "in stop pending",
                    ServiceState.ContinuePending => "in continue pending",
                    _ => "in unknown state",
                };
                Console.WriteLine($"Service {servicename} is {descr}");
                return StarterArgumensResult.ExitNoError;
            }
            if (WindowsServiceHelpers.IsWindowsService())
                return StarterArgumensResult.Run;
            if (console)
                return StarterArgumensResult.Run;

            return StarterArgumensResult.HelpNoError;
        }

        public void ShowHelp()
        {
            Console.WriteLine(VersionHelper.Version);
            Console.WriteLine("Using:\t{0} <arg>", file);
            Console.WriteLine("where <arg> - are:");
            Console.WriteLine("-h, -?, --help                    Show this help and exit");
            Console.WriteLine("-i, --install [<StartUpMode>]     Install service and select start-up mode: Manual/Automatic/Disabled");
            Console.WriteLine("-u, --uninstall                   Uninstall service");
            Console.WriteLine("-s, --start                       Run service");
            Console.WriteLine("-k, --kill, --stop                Stop service");
            Console.WriteLine("-r, --restart                     Restart service");
            Console.WriteLine("-t, --status                      Get service status");
            Console.WriteLine("-c, --console                     Run as console app");
        }

        private static void MooW()
        {
            Console.Write(@"                 (__)
                 (oo)
           /------\/
          / |    ||
         *  /\W--/\
            ~~   ~~
...""Have you mooed today ? ""...");
        }

        public async Task<StarterRunResult> ProcessHostRunAsync(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var line in VersionHelper.VersionLines)
                    logger.Info(line);

                var hostbuilder = CreateHostBuilder(HostArgs.ToArray());
                if (WindowsServiceHelpers.IsWindowsService())
                {
                    logger.Info("Service execution");
                    hostbuilder.Build().Run();
                    return StarterRunResult.Success;
                }
                if (Debugger.IsAttached || console)
                {
                    logger.Info("Console execution");
                    await hostbuilder.RunConsoleAsync(cancellationToken);
                    return StarterRunResult.Success;
                }

                Console.WriteLine($"Try `{file} --help'");
                return StarterRunResult.Success;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to start");
                return StarterRunResult.Error;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private IHostBuilder CreateHostBuilder(string[] args)
        {
            var socketPathProvider = new SocketPathProvider();
            
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

            if (debug)
                hostbuilder.UseEnvironment(Environments.Development);

            logger.Info("Running {0}", Environment.OSVersion.VersionString);
            hostbuilder = hostbuilder.UseWindowsService();
            logger.Info("Using Windows Service");
            logger.Info("Using socket path: {0}", socketPathProvider.GetSocketPath());

            var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
            var pathToContentRoot = Path.GetDirectoryName(pathToExe);
            Directory.SetCurrentDirectory(pathToContentRoot);

            if (File.Exists(socketPathProvider.GetSocketPath()))
                File.Delete(socketPathProvider.GetSocketPath());

            hostbuilder = hostbuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TStartup>();
                webBuilder.ConfigureKestrel(options =>
                {
                    var configuration = options.ApplicationServices.GetService(typeof(IConfiguration)) as IConfiguration;
                    options.ListenUnixSocket(socketPathProvider.GetSocketPath(), options =>
                    {
#if HTTPS
                        var path = configuration["Kestrel:EndpointDefaults:Certificate:Path"];
                        var password = configuration["Kestrel:EndpointDefaults:Certificate:Password"];
                        options.UseHttps(path, password);
#endif
                        options.Protocols = HttpProtocols.Http2;
                    });
                });
            });

            return hostbuilder;
        }
    }
}
