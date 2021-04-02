using APMData.Proto;
using APMOkSvc.Types;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Empty = APMData.Proto.Empty;

namespace APMOkSvc.Services
{
    public class APMDataService : DiskInfoService.DiskInfoServiceBase
    {
        private readonly ILogger _logger;
        public APMDataService(ILogger<APMDataService> logger)
        {
            _logger = logger;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public override Task<SystemDiskInfoReply> EnumerateDisks(Empty request, ServerCallContext context)
        {
            int res = HW.EnumerateDisks(out var diskInfoEnum);

            SystemDiskInfoReply reply = new()
            {
                ResponseTimeStamp = Timestamp.FromDateTime(DateTime.Now),
            };
            if (res == 0)
            {
                reply.ResponseResult = 1;
                return Task.FromResult(reply);
            }
            else
            {
                var diskInfos = diskInfoEnum.Select(item => new DiskInfoEntry()
                {
                    DiskIndex = item.DiskIndex,
                    Index = item.Index,
                    Availability = item.Availability,
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

                var diskInfo = diskInfoEnum.Take(res);
                foreach (var di in diskInfo)
                {
                    _logger.LogTrace(di.Caption);
                }
                return Task.FromResult(reply);
            }
            // return base.EnumerateDisks(request, context);
        }
    }
}
