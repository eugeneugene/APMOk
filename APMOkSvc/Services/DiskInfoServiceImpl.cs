﻿using APMData.Proto;
using APMOkSvc.Code;
using APMOkSvc.Types;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq;

namespace APMOkSvc.Services
{
    public class DiskInfoServiceImpl
    {
        private readonly ILogger _logger;

        public DiskInfoServiceImpl(ILogger<DiskInfoServiceImpl> logger)
        {
            _logger = logger;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public SystemDiskInfoReply EnumerateDisks()
        {
            var reply = new SystemDiskInfoReply
            {
                ReplyTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
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

                    if (HW.LastWin32Error != 0)
                    {
                        var ex = new Win32Exception(HW.LastWin32Error);
                        _logger.LogError(ex.Message);
                    }
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
                    {
                        _logger.LogTrace(di.InfoValid ? di.Caption : "<Invalid>");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {0}", ex);
            }

            _logger.LogTrace("Reply: {0}", reply);
            return reply;
        }

        public GetAPMReply GetAPM(GetAPMRequest request)
        {
            _logger.LogTrace("Request: {0}", request);
            var reply = new GetAPMReply { APMValue = 0, ReplyResult = 0, ReplyTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow) };
            try
            {
                if (HW.GetAPM(request.DiskName, out uint apmValue))
                {
                    reply.APMValue = apmValue;
                    reply.ReplyResult = 1;
                }
                else
                    _logger.LogTrace("GetAPM returned false");
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {0}", ex);
                if (HW.LastWin32Error != 0)
                {
                    ex = new Win32Exception(HW.LastWin32Error);
                    _logger.LogError(ex.Message);
                }
            }
            _logger.LogTrace("Reply: {0}", reply);
            return reply;
        }

        public SetAPMReply SetAPM(SetAPMRequest request)
        {
            _logger.LogTrace("Request: {0}", request);
            var reply = new SetAPMReply { ReplyResult = 0, ReplyTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow) };
            try
            {
                byte val = request.APMValue > 254 ? (byte)0 : (byte)request.APMValue;
                bool disable = request.APMValue > 254;
                if (HW.SetAPM(request.DiskName, val, disable))
                {
                    var newReply = GetAPM(new GetAPMRequest { DiskName = request.DiskName });
                    if (newReply.ReplyResult != 0)
                        _logger.LogTrace("New APM Value: {0}", newReply.APMValue);
                    else
                        _logger.LogTrace("New APM Value is not known");
                    reply.ReplyResult = 1;
                }
                else
                    _logger.LogTrace("SetAPM returned false");
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {0}", ex);
                if (HW.LastWin32Error != 0)
                {
                    ex = new Win32Exception(HW.LastWin32Error);
                    _logger.LogError(ex.Message);
                }
            }
            _logger.LogTrace("Reply: {0}", reply);
            return reply;
        }
    }
}
