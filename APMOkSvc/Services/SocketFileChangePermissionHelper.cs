using APMOkLib;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace APMOkSvc.Services;

internal class SocketFileChangePermissionHelper : BackgroundService
{
    public readonly ILogger _logger;
    public readonly ISocketPathProvider _socketPathProvider;

    public SocketFileChangePermissionHelper(ILogger<SocketFileChangePermissionHelper> logger, ISocketPathProvider socketPathProvider)
    {
        _logger = logger;
        _socketPathProvider = socketPathProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            if (File.Exists(_socketPathProvider.GetSocketPath()))
            {
                GrantFullAccess(_socketPathProvider.GetSocketPath());
                _logger.LogTrace("Successfully set file permissions");
                return;
            }
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private void GrantFullAccess(string fullPath)
    {
        try
        {
            FileInfo fInfo = new(fullPath);
            FileSecurity fSecurity = fInfo.GetAccessControl();
            fSecurity.AddAccessRule(new FileSystemAccessRule(
                identity: new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                fileSystemRights: FileSystemRights.FullControl,
                inheritanceFlags: InheritanceFlags.None,
                propagationFlags: PropagationFlags.NoPropagateInherit,
                type: AccessControlType.Allow));
            fInfo.SetAccessControl(fSecurity);
        }
        catch (Exception ex)
        {
            _logger.LogError("{ex}", ex);
        }
    }
}
