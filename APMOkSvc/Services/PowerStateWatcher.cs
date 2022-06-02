using APMData;
using APMOkSvc.Data;
using APMOkSvc.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services;

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
        _logger.LogTrace("Creating {Name}", GetType().Name);
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

    private async void PowerStateChangeDelegate(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            await PowerStateChangeAsync(sender, e, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError("{ex}", ex);
        }
    }

    private async Task PowerStateChangeAsync(object? sender, PropertyChangedEventArgs e, CancellationToken cancellationToken)
    {
        if (e.PropertyName == "PowerState")
        {
            _logger.LogDebug("PowerState has changed");
            var reply = _diskInfoServiceImpl.EnumerateDisks();
            if (reply.ReplyResult != 0)
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Found disks:");
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
    }

    private void SetAPM(string DeviceID, uint APMValue)
    {
        if (APMValue == 0U)
        {
            _logger.LogWarning("Skip setting APM Value: {0} for {1}", APMValue, DeviceID);
            return;
        }
        byte val = APMValue > 254 ? (byte)0 : (byte)APMValue;
        bool disable = APMValue > 254;
        if (HW.SetAPM(DeviceID, val, disable))
            _logger.LogDebug("APM set successfully for {0} value {1} ({1})", DeviceID, APMValue, disable ? "disabled" : "enabled");
        else
            _logger.LogWarning("Failed to set APM for {0} value {1}", DeviceID, APMValue);
    }
}
