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

        public override Task<SystemDiskInfoReply> EnumerateDisks(Empty request, ServerCallContext context)
        {
            var reply = _diskInfoServiceImpl.EnumerateDisks();
            return Task.FromResult(reply);
        }

        public override Task<GetAPMReply> GetAPM(GetAPMRequest request, ServerCallContext context)
        {
            return Task.FromResult(_diskInfoServiceImpl.GetAPM(request));
        }

        public override Task<SetAPMReply> SetAPM(SetAPMRequest request, ServerCallContext context)
        {
            return Task.FromResult(_diskInfoServiceImpl.SetAPM(request));
        }
    }
}
