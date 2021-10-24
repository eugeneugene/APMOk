using APMData;
using APMOkSvc.Code;
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
#if DEBUG
        private readonly TestDriveService _testDriveService;
#endif

        public DiskInfoServiceImpl(ILogger<DiskInfoServiceImpl> logger
#if DEBUG
           , TestDriveService testDriveService
#endif
           )
        {
            _logger = logger;
#if DEBUG
            _testDriveService = testDriveService;
#endif
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

                if (res != 0 || diskInfoEnum is null)
                {
                    if (EnumerateDisksErrors.Errors.ContainsKey(res))
                        _logger.LogError("Hardware error: {0}", EnumerateDisksErrors.Errors[res]);
                    else
                        _logger.LogError("Hardware error: {0}", res);
                }
                else
                {
                    var diskInfos = diskInfoEnum.Where(item => item.InfoValid != 0).Select(item => new DiskInfoEntry
                    {
                        Index = item.Index,
                        Availability = item.Availability,
                        Caption = item.Caption,
                        Description = item.Description,
                        DeviceID = item.DeviceID,
                        InterfaceType = item.InterfaceType,
                        Manufacturer = item.Manufacturer,
                        Model = item.Model,
                        SerialNumber = item.SerialNumber,
                    }).ToList();

#if DEBUG
                    diskInfos.Add(_testDriveService.TestDriveDiskInfoEntry);
#endif

                    reply.DiskInfoEntries.AddRange(diskInfos);
                    reply.ReplyResult = 1;

                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        foreach (var di in diskInfos)
                            _logger.LogTrace(di.Caption);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0}", ex);
            }

            _logger.LogTrace("Reply: {0}", reply);

            return reply;
        }
    }
}
