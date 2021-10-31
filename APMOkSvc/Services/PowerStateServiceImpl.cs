using APMData;
using APMOkSvc.Types;
using Microsoft.Extensions.Logging;
using System;

namespace APMOkSvc.Services
{
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
            _logger.LogTrace("Creating {0}", GetType().Name);
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
                        _logger.LogTrace("ACLineStatus : {0}", powerState.ACLineStatus);
                        _logger.LogTrace("BatteryFlag : {0}", powerState.BatteryFlag);
                        _logger.LogTrace("BatteryFullLifeTime : {0}", powerState.BatteryFullLifeTime);
                        _logger.LogTrace("BatteryLifePercent : {0}", powerState.BatteryLifePercent);
                        _logger.LogTrace("BatteryLifeTime : {0}", powerState.BatteryLifeTime);
                    }

                    reply.PowerState = new()
                    {
                        PowerSource = powerState.ACLineStatus switch
                        {
                            ACLineStatus.Offline => EPowerSource.Battery,
                            ACLineStatus.Online => EPowerSource.Mains,
                            _ => EPowerSource.Unknown,
                        }
                    };
                    reply.PowerState.BatteryFlag = (EBatteryFlag)powerState.BatteryFlag;
                    reply.PowerState.BatteryFullLifeTime = powerState.BatteryFullLifeTime;
                    reply.PowerState.BatteryLifePercent = powerState.BatteryLifePercent;
                    reply.PowerState.BatteryLifeTime = powerState.BatteryLifeTime;
                    reply.ReplyResult = 1;
                }
                else
                    _logger.LogTrace("GetPowerState returned null");
            }
            catch (Exception ex)
            {
                _logger.LogError("{0}", ex);
            }

            _logger.LogTrace("Reply: {0}", reply);

            return reply;
        }
    }
}
