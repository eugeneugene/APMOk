﻿using APMData;
using APMOkSvc.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    /// <summary>
    /// Implementation of Configuration GRPC Service
    /// DI Lifetime: Transient
    /// </summary>
    public class ConfigurationServiceImpl
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
#if DEBUG
        private readonly TestDriveService _testDriveService;
#endif

        public ConfigurationServiceImpl(ILogger<ConfigurationServiceImpl> logger, IServiceScopeFactory serviceScopeFactory
#if DEBUG
           , TestDriveService testDriveService
#endif
        )
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
#if DEBUG
            _testDriveService = testDriveService;
#endif
            _logger.LogTrace("Creating {0}", GetType().Name);
        }

        public DriveAPMConfigurationReply GetDriveAPMConfiguration()
        {
            var reply = new DriveAPMConfigurationReply
            {
                ReplyResult = 0
            };
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                reply.DriveAPMConfigurationReplyEntries.AddRange(dataContext.ConfigDataSet.Select(item => new DriveAPMConfigurationReplyEntry
                {
                    DeviceID = item.DeviceID,
                    OnMains = item.OnMains,
                    OnBatteries = item.OnBatteries,
                }));

#if DEBUG
                reply.DriveAPMConfigurationReplyEntries.Add(new DriveAPMConfigurationReplyEntry
                {
                    DeviceID = _testDriveService.TestDriveDiskInfoEntry.DeviceID,
                    OnMains = _testDriveService.OnMainsApmValue,
                    OnBatteries = _testDriveService.OnBatteriesApmValue,
                });
#endif
                reply.ReplyResult = 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            _logger.LogTrace("Reply: {0}", reply);

            return reply;
        }

        public async Task<ResetDriveReply> ResetDriveAPMConfigurationAsync(ResetDriveRequest request, CancellationToken cancellationToken)
        {
            var reply = new ResetDriveReply
            {
                ReplyResult = 0
            };
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                var configItem = await dataContext.ConfigDataSet.SingleOrDefaultAsync(item => item.DeviceID == request.DeviceID, cancellationToken);
                if (configItem is not null)
                {
                    switch (request.PowerSource)
                    {
                        case EPowerSource.Mains:
                            configItem.OnMains = 0U;
                            break;
                        case EPowerSource.Battery:
                            configItem.OnBatteries = 0U;
                            break;
                    }
                    dataContext.ConfigDataSet.Update(configItem);
                    _logger.LogTrace("Updated item: {0}", configItem);
                    var records = await dataContext.SaveChangesAsync(cancellationToken);
                    _logger.LogTrace("Updated: {0} records", records);
                    reply.ReplyResult = 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            _logger.LogTrace("Reply: {0}", reply);

            return reply;
        }
    }
}
