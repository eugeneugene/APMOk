using APMData.Proto;
using APMOkSvc.Data;
using APMOkSvc.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    /// <summary>
    /// Startup Hosted Service
    /// DI Lifetime: Singleton (Hosted)
    /// </summary>
    public class StartupHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly DiskInfoServiceImpl _diskInfoServiceImpl;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly PowerStateServiceImpl _powerStateServiceImpl;
        public StartupHostedService(ILogger<StartupHostedService> logger, IServiceScopeFactory serviceScopeFactory, DiskInfoServiceImpl diskInfoServiceImpl,
            IHostApplicationLifetime appLifetime, PowerStateServiceImpl powerStateServiceImpl)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _diskInfoServiceImpl = diskInfoServiceImpl;
            _appLifetime = appLifetime;
            _powerStateServiceImpl = powerStateServiceImpl;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var reply = _diskInfoServiceImpl.EnumerateDisks();
            if (reply.ReplyResult != 0)
            {
                _logger.LogTrace("Обнаружены диски:");
                foreach (var disk in reply.DiskInfoEntries.Where(item => item.InfoValid))
                    _logger.LogTrace("{0}, disk");

                var powerStateReply = _powerStateServiceImpl.GetPowerState();
                if (powerStateReply.ReplyResult != 0)
                {
                    _logger.LogTrace("PowerState: {0}", powerStateReply.PowerState);

                    var scope = _serviceScopeFactory.CreateScope();
                    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                    foreach (var disk in reply.DiskInfoEntries.Where(item => item.InfoValid))
                    {
                        if (await dataContext.ConfigDataSet.AnyAsync(item => item.DeviceID == disk.DeviceID, cancellationToken))
                        {
                            var configData = await dataContext.ConfigDataSet.SingleAsync(item => item.DeviceID == disk.DeviceID, cancellationToken);
                            switch (powerStateReply.PowerState.PowerSource)
                            {
                                case EPowerSource.Mains:
                                    SetAPM(disk.DeviceID, configData.OnMains);
                                    break;
                                case EPowerSource.Battery:
                                    SetAPM(disk.DeviceID, configData.OnBatteries);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void SetAPM(string DeviceID, uint APMValue)
        {
            byte val = APMValue > 254 ? (byte)0 : (byte)APMValue;
            bool disable = APMValue > 254;
            if (HW.SetAPM(DeviceID, val, disable))
                _logger.LogDebug("APM set successfully");
            else
                _logger.LogWarning("Failed to set APM");
        }
    }
}
