using APMData.Proto;
using APMOkSvc.Types;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace APMData
{
    public partial class DiskInfoService : Proto.DiskInfoService.DiskInfoServiceBase
    {
        private readonly ILogger _logger;
        public DiskInfoService(ILogger<DiskInfoService> logger)
        {
            _logger = logger;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public override Task<SystemDiskInfoReply> EnumerateDisks(Empty request, ServerCallContext context)
        {
            int res = HW.EnumerateDisks(out var diskInfoEnum);

            SystemDiskInfoReply reply = new()
            {
                ResponseTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
            };
            if (res != 0)
            {
                if (EnumerateDisksErrors.ContainsKey(res))
                    _logger.LogError("Hardware error: {0}", EnumerateDisksErrors[res]);
                else
                    _logger.LogError("Hardware error: {0}", res);

                if (HW.LastWin32Error != 0)
                {
                    var ex = new Win32Exception(HW.LastWin32Error);
                    _logger.LogError(ex.Message);
                }
                reply.ResponseResult = res;
                return Task.FromResult(reply);
            }
            else
            {
                var diskInfos = diskInfoEnum.Select(item => new Proto.DiskInfoEntry()
                {
                    InfoValid = item.InfoValid != 0,
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

                foreach (var di in diskInfos)
                {
                    _logger.LogTrace(di.InfoValid ? di.Caption : "<Invalid>");
                }
                return Task.FromResult(reply);
            }
        }
    }
}
