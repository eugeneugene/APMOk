using APMData;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace APMOkSvc.Services;

/// <summary>
/// Version GRPC Service
/// </summary>
public class VersionGRPCService : VersionService.VersionServiceBase
{
    private readonly ILogger _logger;
    private readonly VersionServiceImpl _versionServiceImpl;

    public VersionGRPCService(ILogger<DiskInfoGRPCService> logger, VersionServiceImpl versionServiceImpl)
    {
        _logger = logger;
        _versionServiceImpl = versionServiceImpl;
        _logger.LogTrace("Creating {Name}", GetType().Name);
    }

    public override Task<ServiceVersionReply> GetServiceVersion(Empty request, ServerCallContext context)
    {
        return Task.FromResult(_versionServiceImpl.GetServiceVersion());
    }
}
