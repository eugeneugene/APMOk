using APMData;
using APMOkSvc.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace APMOkSvc.Services
{
    /// <summary>
    /// Implementation of Configuration GRPC Service
    /// DI Lifetime: Transient
    /// </summary>
    public class ConfigurationServiceImpl
    {
        private readonly ILogger _logger;
        private readonly DataContext _dataContext;

        public ConfigurationServiceImpl(ILogger<ConfigurationServiceImpl> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
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
                reply.DriveAPMConfigurationReplyEntries.AddRange(_dataContext.ConfigDataSet.Select(item => new DriveAPMConfigurationReplyEntry
                {
                    DeviceID = item.DeviceID,
                    OnMains = item.OnMains,
                    OnBatteries = item.OnBatteries,
                }));
                reply.ReplyResult = 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return reply;
        }
    }
}
