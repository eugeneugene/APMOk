using APMData.Proto;
using APMOkSvc.Code;
using APMOkSvc.Types;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    public class DiskInfoService : APMData.Proto.DiskInfoService.DiskInfoServiceBase
    {
        private readonly ILogger _logger;

        public DiskInfoService(ILogger<DiskInfoService> logger)
        {
            _logger = logger;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }
       
        public override Task<SystemDiskInfoReply> EnumerateDisks(Empty request, ServerCallContext context)
        {
            var reply = EnumerateDisks();
            return Task.FromResult(reply);
        }

        public SystemDiskInfoReply EnumerateDisks()
        {
            SystemDiskInfoReply reply = new()
            {
                ReplyTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
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
                    reply.ReplyResult = res;
                }
                else
                {
                    var diskInfos = diskInfoEnum.Select(item => new DiskInfoEntry()
                    {
                        InfoValid = item.InfoValid != 0,
                        Index = item.Index,
                        Availability = item.Availability,
                        APMValue = item.APMValue,
                        Caption = item.Caption,
                        ConfigManagerErrorCode = item.ConfigManagerErrorCode,
                        Description = item.Description,
                        DeviceID = item.DeviceID,
                        InterfaceType = item.InterfaceType,
                        Manufacturer = item.Manufacturer,
                        Model = item.Model,
                        Name = item.Name,
                        SerialNumber = item.SerialNumber,
                        Status = item.Status,
                    });

                    reply.DiskInfoEntries.AddRange(diskInfos);

                    foreach (var di in diskInfos)
                    {
                        _logger.LogTrace(di.InfoValid ? di.Caption : "<Invalid>");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {0}", ex);
                reply.ReplyResult = 0;
            }
            return reply;
        }
        
        public override Task<GetAPMReply> GetAPM(GetAPMRequest request, ServerCallContext context)
        {
            try
            {
                if (HW.GetAPM(request.DiskId, out int apmValue))
                    return Task.FromResult<GetAPMReply>(new() { APMValue = apmValue, ReplyResult = 1, ReplyTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow), });
            }
            catch(Exception ex)
            {
                _logger.LogError("Exception: {0}", ex);
            }
            return Task.FromResult<GetAPMReply>(new() { APMValue = 0, ReplyResult = 0, ReplyTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow), });
        }
    }
}
