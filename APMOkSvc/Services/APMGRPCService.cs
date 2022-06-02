using APMData;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services;

/// <summary>
/// APM GRPC Service
/// </summary>
public class APMGRPCService : APMService.APMServiceBase
{
    private readonly ILogger _logger;
    private readonly APMServiceImpl _apmServiceImpl;

    public APMGRPCService(ILogger<DiskInfoGRPCService> logger, APMServiceImpl apmServiceImpl)
    {
        _logger = logger;
        _apmServiceImpl = apmServiceImpl;
        _logger.LogTrace("Creating {Name}", GetType().Name);
    }

    public override Task<CurrentAPMReply> GetCurrentAPM(CurrentAPMRequest request, ServerCallContext context)
    {
        return Task.FromResult(_apmServiceImpl.GetCurrentAPM(request));
    }

    public override async Task<APMReply> SetAPM(APMRequest request, ServerCallContext context)
    {
        return await _apmServiceImpl.SetAPMAsync(request, CancellationToken.None);
    }
}
