using APMData;
using APMOkSvc.Code;
using APMOkSvc.Data;
using APMOkSvc.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace APMOkSvc.Services
{
    /// <summary>
    /// Implementation of DiskInfo GRPC Service
    /// DI Lifetime: Transient
    /// </summary>
    public class DiskInfoServiceImpl
    {
        private readonly ILogger _logger;

        public DiskInfoServiceImpl(ILogger<DiskInfoServiceImpl> logger)
        {
            _logger = logger;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public DisksReply EnumerateDisks()
        {
            var reply = new DisksReply
            {
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
                        _logger.LogTrace(di.InfoValid ? di.Caption : "<Invalid>");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {0}", ex);
            }

            _logger.LogTrace("Reply: {0}", reply);
            return reply;
        }
    }
}
