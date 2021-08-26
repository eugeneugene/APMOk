using APMData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    /// <summary>
    /// DiskInfo GRPC Service
    /// </summary>
    public class DiskInfoGRPCService : DiskInfoService.DiskInfoServiceBase
    {
        private readonly ILogger _logger;
        private readonly DiskInfoServiceImpl _diskInfoServiceImpl;

        public DiskInfoGRPCService(ILogger<DiskInfoGRPCService> logger, DiskInfoServiceImpl diskInfoServiceImpl)
        {
            _logger = logger;
            _diskInfoServiceImpl = diskInfoServiceImpl;
            _logger.LogTrace("Создание экземпляра {0}", GetType().Name);
        }

        public override Task<DisksReply> EnumerateDisks(Empty request, ServerCallContext context)
        {
            var reply = _diskInfoServiceImpl.EnumerateDisks();
            return Task.FromResult(reply);
        }
    }
}
