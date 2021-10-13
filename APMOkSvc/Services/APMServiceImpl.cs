using APMData;
using APMOkSvc.Data;
using APMOkSvc.Types;
using APMOkSvc.Types.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    /// <summary>
    /// Implementation of APM GRPC Service
    /// DI Lifetime: Transient
    /// </summary>
    public class APMServiceImpl
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly PowerStateContainer _powerStatusContainer;
        private readonly TestDriveService _testDriveService;

        public APMServiceImpl(ILogger<APMServiceImpl> logger, IServiceScopeFactory serviceScopeFactory, PowerStateContainer powerStatusContainer, TestDriveService testDriveService)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _powerStatusContainer = powerStatusContainer;
            _testDriveService = testDriveService;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public CurrentAPMReply GetCurrentAPM(CurrentAPMRequest request)
        {
            _logger.LogTrace("Request: {0}", request);
            var reply = new CurrentAPMReply { APMValue = 0, ReplyResult = 0, PowerSource = EPowerSource.Unknown, };
            try
            {
                if (request.DeviceID == _testDriveService.TestDriveDiskInfoEntry.DeviceID)
                {
                    reply.APMValue = _powerStatusContainer.PowerState.PowerState.PowerSource switch
                    {
                        EPowerSource.Mains => _testDriveService.OnMainsApmValue,
                        EPowerSource.Battery => _testDriveService.OnBatteriesApmValue,
                        _ => 0U,
                    };
                    reply.PowerSource = _powerStatusContainer.PowerState.PowerState.PowerSource;
                    reply.ReplyResult = 1;
                }
                else if (HW.GetAPM(request.DeviceID, out uint apmValue))
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
                _logger.LogError("{0}", ex);
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

                if (request.DeviceID == _testDriveService.TestDriveDiskInfoEntry.DeviceID)
                {
                    if (request.PowerSource == EPowerSource.Mains)
                        _testDriveService.OnMainsApmValue = request.APMValue;
                    if (request.PowerSource == EPowerSource.Battery)
                        _testDriveService.OnBatteriesApmValue = request.APMValue;
                    reply.ReplyResult = 1;
                }
                else
                {
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

                    using var scope = _serviceScopeFactory.CreateScope();
                    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                    if (dataContext.ConfigDataSet.Any(item => item.DeviceID == request.DeviceID))
                    {
                        var configData = dataContext.ConfigDataSet.Single(item => item.DeviceID == request.DeviceID);
                        switch (powerSource)
                        {
                            case EPowerSource.Mains:
                                configData.OnMains = request.APMValue;
                                dataContext.ConfigDataSet.Update(configData);
                                break;
                            case EPowerSource.Battery:
                                configData.OnBatteries = request.APMValue;
                                dataContext.ConfigDataSet.Update(configData);
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
                        dataContext.ConfigDataSet.Add(new ConfigData(request.DeviceID, OnMains, OnBatteries));
                    }
                    dataContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0}", ex);
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

                if (request.DeviceID == _testDriveService.TestDriveDiskInfoEntry.DeviceID)
                {
                    if (request.PowerSource == EPowerSource.Mains)
                        _testDriveService.OnMainsApmValue = request.APMValue;
                    if (request.PowerSource == EPowerSource.Battery)
                        _testDriveService.OnBatteriesApmValue = request.APMValue;
                    reply.ReplyResult = 1;
                }
                else
                {
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

                    using var scope = _serviceScopeFactory.CreateScope();
                    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                    if (await dataContext.ConfigDataSet.AnyAsync(item => item.DeviceID == request.DeviceID, cancellationToken))
                    {
                        var configData = await dataContext.ConfigDataSet.SingleAsync(item => item.DeviceID == request.DeviceID, cancellationToken);
                        switch (powerSource)
                        {
                            case EPowerSource.Mains:
                                configData.OnMains = request.APMValue;
                                dataContext.ConfigDataSet.Update(configData);
                                break;
                            case EPowerSource.Battery:
                                configData.OnBatteries = request.APMValue;
                                dataContext.ConfigDataSet.Update(configData);
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
                        dataContext.ConfigDataSet.Add(new ConfigData(request.DeviceID, OnMains, OnBatteries));
                    }
                    await dataContext.SaveChangesAsync(cancellationToken);
                }
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
