using APMData;
using APMOkSvc.Data;
using APMOkSvc.Types;
using APMOkSvc.Types.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services;

/// <summary>
/// Implementation of APM GRPC Service
/// DI Lifetime: Transient
/// </summary>
public class APMServiceImpl
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly PowerStateContainer _powerStatusContainer;
#if DEBUG
    private readonly TestDriveService _testDriveService;
#endif

#if DEBUG
    public APMServiceImpl(ILogger<APMServiceImpl> logger, IServiceScopeFactory serviceScopeFactory, PowerStateContainer powerStatusContainer, TestDriveService testDriveService)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _powerStatusContainer = powerStatusContainer;
        _testDriveService = testDriveService;
        _logger.LogTrace("Creating {Name}", GetType().Name);
    }
#else
    public APMServiceImpl(ILogger<APMServiceImpl> logger, IServiceScopeFactory serviceScopeFactory, PowerStateContainer powerStatusContainer)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _powerStatusContainer = powerStatusContainer;
        _logger.LogTrace("Creating {Name}", GetType().Name);
    }
#endif

    public CurrentAPMReply GetCurrentAPM(CurrentAPMRequest request)
    {
        _logger.LogTrace("Request: {request}", request);
        var reply = new CurrentAPMReply { APMValue = 0, ReplyResult = 0, PowerSource = EPowerSource.Unknown, };
        try
        {
#if DEBUG
            if (request.DeviceID == _testDriveService.TestDriveDiskInfoEntry.DeviceID)
            {
                reply.APMValue = _powerStatusContainer.PowerState?.PowerState.PowerSource switch
                {
                    EPowerSource.Mains => _testDriveService.OnMainsApmValue,
                    EPowerSource.Battery => _testDriveService.OnBatteriesApmValue,
                    _ => 0U,
                };
                reply.PowerSource = _powerStatusContainer.PowerState?.PowerState.PowerSource ?? EPowerSource.Unknown;
                reply.ReplyResult = 1;
            }
            else
#endif
            if (HW.GetAPM(request.DeviceID, out uint apmValue))
            {
                reply.APMValue = apmValue;
                reply.PowerSource = _powerStatusContainer.PowerState?.PowerState.PowerSource ?? EPowerSource.Unknown;
                reply.ReplyResult = 1;
            }
            else
                _logger.LogTrace("GetAPM returned false");
        }
        catch (Win32Exception wex)
        {
            _logger.LogError("Win32Exception: '{message}' DeviceId: {device}", wex.Message, request.DeviceID);
        }
        catch (Exception ex)
        {
            _logger.LogError("{ex}", ex);
        }

        _logger.LogTrace("Reply: {reply}", reply);

        return reply;
    }

    public async Task<APMReply> SetAPMAsync(APMRequest request, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Request: {request}", request);
        var reply = new APMReply { ReplyResult = 0, };
        try
        {
            var powerSource = _powerStatusContainer.PowerState?.PowerState.PowerSource ?? EPowerSource.Unknown;

#if DEBUG
            if (request.DeviceID == _testDriveService.TestDriveDiskInfoEntry.DeviceID)
            {
                if (request.PowerSource == EPowerSource.Mains)
                    _testDriveService.OnMainsApmValue = request.APMValue;
                if (request.PowerSource == EPowerSource.Battery)
                    _testDriveService.OnBatteriesApmValue = request.APMValue;
                reply.ReplyResult = 1;
            }
            else
#endif
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
                else
                    reply.ReplyResult = 1;

                using var scope = _serviceScopeFactory.CreateScope();
                var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                if (await dataContext.ConfigDataSet.AnyAsync(item => item.DeviceID == request.DeviceID, cancellationToken))
                {
                    var configData = await dataContext.ConfigDataSet.SingleAsync(item => item.DeviceID == request.DeviceID, cancellationToken);
                    switch (request.PowerSource)
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
                    _logger.LogTrace("Updated: {data}", configData);
                }
                else
                {
                    uint OnMains = 0U;
                    uint OnBatteries = 0U;
                    switch (request.PowerSource)
                    {
                        case EPowerSource.Mains:
                            OnMains = request.APMValue;
                            break;
                        case EPowerSource.Battery:
                            OnBatteries = request.APMValue;
                            break;
                    }
                    var configData = new ConfigData(request.DeviceID, OnMains, OnBatteries);
                    dataContext.ConfigDataSet.Add(configData);
                    _logger.LogTrace("Added: {data}", configData);
                }
                var records = await dataContext.SaveChangesAsync(cancellationToken);
                _logger.LogTrace("Added: {records} records", records);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("{ex}", ex);
        }

        _logger.LogTrace("Reply: {reply}", reply);

        return reply;
    }
}
