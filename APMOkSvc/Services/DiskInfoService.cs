using APMData.Proto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace APMOkSvc.Services
{
    public class DiskInfoService : APMData.Proto.DiskInfoService.DiskInfoServiceBase
    {
        private readonly ILogger _logger;
        private readonly DiskInfoServiceImpl _diskInfoServiceImpl;

        public DiskInfoService(ILogger<DiskInfoService> logger, DiskInfoServiceImpl diskInfoServiceImpl)
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

        public override Task<CurrentAPMReply> GetCurrentAPM(CurrentAPMRequest request, ServerCallContext context)
        {
            return Task.FromResult(_diskInfoServiceImpl.GetCurrentAPM(request));
        }

        public override Task<APMReply> SetAPM(APMRequest request, ServerCallContext context)
        {
            return Task.FromResult(_diskInfoServiceImpl.SetAPM(request));
        }
    }
}
