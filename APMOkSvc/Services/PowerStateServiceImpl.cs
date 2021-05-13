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
            var reply = new PowerStateReply()
            {
                ReplyResult = 0,
            };
            try
            {
                var powerState = PowerState.GetPowerState();
                if (powerState != null)
                {
                    _logger.LogTrace("PowerState:");
                    _logger.LogTrace("ACLineStatus : {0}", powerState.ACLineStatus);
                    _logger.LogTrace("BatteryFlag : {0}", powerState.BatteryFlag);
                    _logger.LogTrace("BatteryFullLifeTime : {0}", powerState.BatteryFullLifeTime);
                    _logger.LogTrace("BatteryLifePercent : {0}", powerState.BatteryLifePercent);
                    _logger.LogTrace("BatteryLifeTime : {0}", powerState.BatteryLifeTime);

                    reply.ReplyResult = 1;
                    reply.ACLineStatus = (EACLineStatus)powerState.ACLineStatus;
                    reply.BatteryFlag = (EBatteryFlag)powerState.BatteryFlag;
                    reply.BatteryFullLifeTime = powerState.BatteryFullLifeTime;
                    reply.BatteryLifePercent = powerState.BatteryLifePercent;
                    reply.BatteryLifeTime = powerState.BatteryLifeTime;
                }
                else
                    _logger.LogTrace("GetPowerState returned null");
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {0}", ex);
            }
            _logger.LogTrace("Reply: {0}", reply);
            return reply;
        }
    }
}
