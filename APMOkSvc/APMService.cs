using APMData;
using APMOkSvc.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace APMOkSvc
{
    [ServiceContract]
    public interface IAPMService
    {
        [OperationContract]
        SystemDiskInfoResponse GetSystemDiskInfo();
    }

    public class APMService : IAPMService
    {
        public readonly ILogger _logger;

        public APMService(ILogger<APMService> logger)
        {
            _logger = logger;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public SystemDiskInfoResponse GetSystemDiskInfo()
        {
            int res = HW.EnumerateDisks(out var diskInfoEnum);

            if (res == 0)
            {
                _logger.LogWarning("No hard drives were found");
                return new()
                {
                    ResponseResult = -1,
                    ResponseTimeStamp = DateTime.Now,
                    DiskInfoEntries = new(),
                };
            }
            else
            {
                List<DiskInfoEntry> diskInfoList = new();

                var diskInfo = diskInfoEnum.Take(res);

                diskInfoList.AddRange(diskInfo.Select(item =>
                {
                    _logger.LogTrace("Found '{0}'", item.Caption);
                    return new DiskInfoEntry()
                    {
                        DiskIndex = item.DiskIndex,
                        Index = item.Index,
                        Availability = item.Availability,
                        Caption = item.Caption,
                        ConfigManagerErrorCode = item.ConfigManagerErrorCode,
                        Description = item.Description,
                        DeviceID=item.DeviceID,
                        InterfaceType=item.InterfaceType,
                        Manufacturer=item.Manufacturer,
                        Model = item.Model,
                        Name = item.Name,
                        SerialNumber = item.SerialNumber,
                        Status = item.Status,
                    };
                }));

                return new()
                {
                    ResponseResult = 0,
                    ResponseTimeStamp = DateTime.Now,
                    DiskInfoEntries = diskInfoList,
                };
            }
        }
    }
}
