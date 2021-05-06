using APMData.Proto;
using APMOkSvc.Types;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using System;

namespace APMOkSvc.Services
{
    public class PowerStateServiceImpl
    {
        private readonly ILogger _logger;

        public PowerStateServiceImpl(ILogger<PowerStateServiceImpl> logger)
        {
            _logger = logger;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public PowerStateReply GetPowerState()
        {
            try
            {
                var powerState = PowerState.GetPowerState();
                _logger.LogTrace("PowerState:");
                _logger.LogTrace("ACLineStatus : {0}", powerState.ACLineStatus);
                _logger.LogTrace("BatteryFlag : {0}", powerState.BatteryFlag);
                _logger.LogTrace("BatteryFullLifeTime : {0}", powerState.BatteryFullLifeTime);
                _logger.LogTrace("BatteryLifePercent : {0}", powerState.BatteryLifePercent);
                _logger.LogTrace("BatteryLifeTime : {0}", powerState.BatteryLifeTime);

                return new PowerStateReply()
                {
                    ReplyResult = 1,
                    ReplyTimeStamp = Timestamp.FromDateTime(DateTime.Now),
                    ACLineStatus = (EACLineStatus)powerState.ACLineStatus,
                    BatteryFlag = (EBatteryFlag)powerState.BatteryFlag,
                    BatteryFullLifeTime = powerState.BatteryFullLifeTime,
                    BatteryLifePercent = powerState.BatteryLifePercent,
                    BatteryLifeTime = powerState.BatteryLifeTime,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {0}", ex);
                return new PowerStateReply()
                {
                    ReplyResult = 0,
                };
            }
        }
    }
}
