using System;
using System.IO;

namespace APMOkLib;

/// <summary>
/// SocketPathProvider
/// DI Lifetime: Transient
/// </summary>
public class SocketPathProvider : ISocketPathProvider
{
    public string GetSocketPath() =>
         Path.Combine(Environment.GetEnvironmentVariable("ProgramData") ?? "C:\\ProgramData", "APMOk", "APMOkSvc.socket");
}
