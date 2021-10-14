using APMData;
using APMOkSvc.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        private readonly IHostEnvironment _environment;
        private readonly TestDriveService _testDriveService;

        public ConfigurationServiceImpl(ILogger<ConfigurationServiceImpl> logger, IServiceScopeFactory serviceScopeFactory,
            IHostEnvironment environment, TestDriveService testDriveService)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _environment = environment;
            _testDriveService = testDriveService;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
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

                if (_environment.IsDevelopment())
                {
                    reply.DriveAPMConfigurationReplyEntries.Add(new DriveAPMConfigurationReplyEntry
                    {
                        DeviceID = _testDriveService.TestDriveDiskInfoEntry.DeviceID,
                        OnMains = _testDriveService.OnMainsApmValue,
                        OnBatteries = _testDriveService.OnBatteriesApmValue,
                    });
                }
                reply.ReplyResult = 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
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
                    dataContext.ConfigDataSet.Remove(configItem);
                    await dataContext.SaveChangesAsync(cancellationToken);
                    reply.ReplyResult = 1;
                    reply.DeviceID = configItem.DeviceID;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return reply;
        }
    }
}
