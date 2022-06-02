using APMData;
using Microsoft.Extensions.Logging;

namespace APMOkSvc.Services;

public class VersionServiceImpl
{
    private readonly ILogger _logger;
    private readonly AssemblyInfo _assemblyInfo;

    public VersionServiceImpl(ILogger<DiskInfoServiceImpl> logger, AssemblyInfo assemblyInfo)
    {
        _logger = logger;
        _assemblyInfo = assemblyInfo;
    }

    public ServiceVersionReply GetServiceVersion()
    {
        ServiceVersionReply reply = new()
        {
            ReplyResult = 0,
            ServiceName = string.Empty,
            Major = 0,
            Minor = 0,
            Build = 0,
            Revision = 0,
        };
        var assemblyName = _assemblyInfo.Assembly.GetName();
        var version = assemblyName.Version;
        if (version is not null)
        {
            reply.ReplyResult = 1;
            reply.ServiceName = assemblyName.Name;
            reply.Major = version.Major;
            reply.Minor = version.Minor;
            reply.Build = version.Build;
            reply.Revision = version.Revision;
        }
        _logger.LogTrace("Reply: {reply}", reply);
        return reply;
    }
}
