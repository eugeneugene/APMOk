using APMData;
using APMOkSvc.Types;
using Microsoft.Extensions.Logging;
using System;

namespace APMOkSvc.Services;

/// <summary>
/// Implementation PowerState GRPC Service
/// DI Lifetime: Transient
/// </summary>
public class PowerStateServiceImpl
{
    private readonly ILogger _logger;

    public PowerStateServiceImpl(ILogger<PowerStateServiceImpl> logger)
    {
        _logger = logger;
        _logger.LogTrace("Creating {Name}", GetType().Name);
    }

    public PowerStateReply GetPowerState()
    {
        var reply = new PowerStateReply()
        {
            ReplyResult = 0,
        };
        try
        {
            var powerState = PowerState.GetPowerState();
            if (powerState != null)
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("PowerState:");
                    _logger.LogTrace("ACLineStatus : {ACLineStatus}", powerState.ACLineStatus);
                    _logger.LogTrace("BatteryFlag : {BatteryFlag}", powerState.BatteryFlag);
                    _logger.LogTrace("BatteryFullLifeTime : {BatteryFullLifeTime}", powerState.BatteryFullLifeTime);
                    _logger.LogTrace("BatteryLifePercent : {BatteryLifePercent}", powerState.BatteryLifePercent);
                    _logger.LogTrace("BatteryLifeTime : {BatteryLifeTime}", powerState.BatteryLifeTime);
                }

                reply.PowerState = new()
                {
                    PowerSource = powerState.ACLineStatus switch
                    {
                        ACLineStatus.Offline => EPowerSource.Battery,
                        ACLineStatus.Online => EPowerSource.Mains,
                        _ => EPowerSource.Unknown,
                    },
                    BatteryFlag = (EBatteryFlag)powerState.BatteryFlag,
                    BatteryFullLifeTime = powerState.BatteryFullLifeTime,
                    BatteryLifePercent = powerState.BatteryLifePercent,
                    BatteryLifeTime = powerState.BatteryLifeTime
                };
                reply.ReplyResult = 1;
            }
            else
                _logger.LogTrace("GetPowerState returned null");
        }
        catch (Exception ex)
        {
            _logger.LogError("{ex}", ex);
        }

        _logger.LogTrace("Reply: {reply}", reply);

        return reply;
    }
}
