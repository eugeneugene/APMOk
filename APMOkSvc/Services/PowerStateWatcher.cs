using APMData;
using APMOkSvc.Data;
using APMOkSvc.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    /// <summary>
    /// Watches the changes of PowerState
    /// DI Lifetime: Singleton (Hosted service)
    /// </summary>
    public class PowerStateWatcher : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly PowerStateContainer _powerStateContainer;
        private readonly DiskInfoServiceImpl _diskInfoServiceImpl;
        private readonly PowerStateServiceImpl _powerStateServiceImpl;

        public PowerStateWatcher(ILogger<PowerStateWatcher> logger, IServiceScopeFactory serviceScopeFactory, PowerStateContainer powerStateContainer,
            DiskInfoServiceImpl diskInfoServiceImpl, PowerStateServiceImpl powerStateServiceImpl)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _powerStateContainer = powerStateContainer;
            _diskInfoServiceImpl = diskInfoServiceImpl;
            _powerStateServiceImpl = powerStateServiceImpl;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _powerStateContainer.PropertyChanged += PowerStateChangeDelegate;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _powerStateContainer.PropertyChanged -= PowerStateChangeDelegate;
            return Task.CompletedTask;
        }

        private void PowerStateChangeDelegate(object sender, PropertyChangedEventArgs e)
        {
            _logger.LogDebug("PowerState has changed");
            if (e.PropertyName == "PowerState")
            {
                var reply = _diskInfoServiceImpl.EnumerateDisks();
                if (reply.ReplyResult != 0)
                {
                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        _logger.LogTrace("Обнаружены диски:");
                        foreach (var disk in reply.DiskInfoEntries)
                            _logger.LogTrace("{0}", disk);
                    }

                    var powerStateReply = _powerStateServiceImpl.GetPowerState();
                    if (powerStateReply.ReplyResult != 0)
                    {
                        _logger.LogTrace("PowerState: {0}", powerStateReply.PowerState);

                        using var scope = _serviceScopeFactory.CreateScope();
                        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                        foreach (var disk in reply.DiskInfoEntries)
                        {
                            if (dataContext.ConfigDataSet.Any(item => item.DeviceID == disk.DeviceID))
                            {
                                var configData = dataContext.ConfigDataSet.Single(item => item.DeviceID == disk.DeviceID);
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
        }

        private void SetAPM(string DeviceID, uint APMValue)
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
