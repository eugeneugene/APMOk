using APMData;
using APMOkSvc.Code;
using APMOkSvc.Data;
using APMOkSvc.Types;
using APMOkSvc.Types.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    /// <summary>
    /// Implementation of DiskInfo GRPC Service
    /// DI Lifetime: Transient
    /// </summary>
    public class DiskInfoServiceImpl
    {
        private readonly ILogger _logger;
        private readonly PowerStatusContainer _powerStatusContainer;
        private readonly DataContext _dataContext;

        public DiskInfoServiceImpl(ILogger<DiskInfoServiceImpl> logger, PowerStatusContainer powerStatusContainer, DataContext dataContext)
        {
            _logger = logger;
            _powerStatusContainer = powerStatusContainer;
            _dataContext = dataContext;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public DisksReply EnumerateDisks()
        {
            var reply = new DisksReply
            {
                ReplyResult = 0
            };

            try
            {
                int res = HW.EnumerateDisks(out var diskInfoEnum);

                if (res != 0)
                {
                    if (EnumerateDisksErrors.Errors.ContainsKey(res))
                        _logger.LogError("Hardware error: {0}", EnumerateDisksErrors.Errors[res]);
                    else
                        _logger.LogError("Hardware error: {0}", res);
                }
                else
                {
                    var diskInfos = diskInfoEnum.Select(item => new DiskInfoEntry
                    {
                        InfoValid = item.InfoValid != 0,
                        Index = item.Index,
                        Availability = item.Availability,
                        Caption = item.Caption,
                        Description = item.Description,
                        DeviceID = item.DeviceID,
                        InterfaceType = item.InterfaceType,
                        Manufacturer = item.Manufacturer,
                        Model = item.Model,
                        SerialNumber = item.SerialNumber,
                    });

                    reply.DiskInfoEntries.AddRange(diskInfos);
                    reply.ReplyResult = 1;

                    foreach (var di in diskInfos)
                        _logger.LogTrace(di.InfoValid ? di.Caption : "<Invalid>");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {0}", ex);
            }

            _logger.LogTrace("Reply: {0}", reply);
            return reply;
        }

        public CurrentAPMReply GetCurrentAPM(CurrentAPMRequest request)
        {
            _logger.LogTrace("Request: {0}", request);
            var reply = new CurrentAPMReply { APMValue = 0, ReplyResult = 0, PowerSource = EPowerSource.Unknown, };
            try
            {
                if (HW.GetAPM(request.DeviceID, out uint apmValue))
                {
                    reply.APMValue = apmValue;
                    reply.PowerSource = _powerStatusContainer.PowerState.PowerState.PowerSource;
                    reply.ReplyResult = 1;
                }
                else
                    _logger.LogTrace("GetAPM returned false");
            }
            catch (Win32Exception wex)
            {
                _logger.LogError("Win32Exception: '{0}' DeviceId: {1}", wex.Message, request.DeviceID);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {0}", ex);
            }
            _logger.LogTrace("Reply: {0}", reply);
            return reply;
        }

        public APMReply SetAPM(APMRequest request)
        {
            _logger.LogTrace("Request: {0}", request);
            var reply = new APMReply { ReplyResult = 0, };
            try
            {
                var powerSource = _powerStatusContainer.PowerState.PowerState.PowerSource;
                if (powerSource == request.PowerSource)
                {
                    byte val = request.APMValue > 254 ? (byte)0 : (byte)request.APMValue;
                    bool disable = request.APMValue > 254;
                    if (HW.SetAPM(request.DeviceID, val, disable))
                    {
                        _logger.LogDebug("APM set successfully");
                        reply.ReplyResult = 1;

                    }
                    else
                        _logger.LogWarning("Failed to set APM");
                }

                if (_dataContext.ConfigDataSet.Any(item => item.DeviceID == request.DeviceID))
                {
                    var configData = _dataContext.ConfigDataSet.Single(item => item.DeviceID == request.DeviceID);
                    switch (powerSource)
                    {
                        case EPowerSource.Mains:
                            configData.OnMains = request.APMValue;
                            _dataContext.ConfigDataSet.Update(configData);
                            break;
                        case EPowerSource.Battery:
                            configData.OnBatteries = request.APMValue;
                            _dataContext.ConfigDataSet.Update(configData);
                            break;
                    }
                }
                else
                {
                    uint OnMains = 0U;
                    uint OnBatteries = 0U;
                    switch (powerSource)
                    {
                        case EPowerSource.Mains:
                            OnMains = request.APMValue;
                            break;
                        case EPowerSource.Battery:
                            OnBatteries = request.APMValue;
                            break;
                    }
                    _dataContext.ConfigDataSet.Add(new ConfigData(request.DeviceID, OnMains, OnBatteries));
                }
                _dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {0}", ex);
            }
            _logger.LogTrace("Reply: {0}", reply);
            return reply;
        }

        public async Task<APMReply> SetAPMAsync(APMRequest request, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Request: {0}", request);
            var reply = new APMReply { ReplyResult = 0, };
            try
            {
                var powerSource = _powerStatusContainer.PowerState.PowerState.PowerSource;
                if (powerSource == request.PowerSource)
                {
                    byte val = request.APMValue > 254 ? (byte)0 : (byte)request.APMValue;
                    bool disable = request.APMValue > 254;
                    if (HW.SetAPM(request.DeviceID, val, disable))
                    {
                        _logger.LogDebug("APM set successfully");
                        reply.ReplyResult = 1;

                    }
                    else
                        _logger.LogWarning("Failed to set APM");
                }

                if (await _dataContext.ConfigDataSet.AnyAsync(item => item.DeviceID == request.DeviceID, cancellationToken))
                {
                    var configData = await _dataContext.ConfigDataSet.SingleAsync(item => item.DeviceID == request.DeviceID, cancellationToken);
                    switch (powerSource)
                    {
                        case EPowerSource.Mains:
                            configData.OnMains = request.APMValue;
                            _dataContext.ConfigDataSet.Update(configData);
                            break;
                        case EPowerSource.Battery:
                            configData.OnBatteries = request.APMValue;
                            _dataContext.ConfigDataSet.Update(configData);
                            break;
                    }
                }
                else
                {
                    uint OnMains = 0U;
                    uint OnBatteries = 0U;
                    switch (powerSource)
                    {
                        case EPowerSource.Mains:
                            OnMains = request.APMValue;
                            break;
                        case EPowerSource.Battery:
                            OnBatteries = request.APMValue;
                            break;
                    }
                    _dataContext.ConfigDataSet.Add(new ConfigData(request.DeviceID, OnMains, OnBatteries));
                }
                _dataContext.SaveChanges();
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
