﻿using APMData.Proto;
using APMOkSvc.Data;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace APMOkSvc.Services
{
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
                ReplyTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                ReplyResult = 0
            };
            try
            {
                reply.DriveAPMConfigurationReplyEntries.AddRange(_dataContext.ConfigDataSet.Select(item => new DriveAPMConfigurationReplyEntry
                {
                    DeviceID = item.DeviceID,
                    CurrentValue = item.CurrentValue,
                    DefaultValue = item.DefaultValue
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