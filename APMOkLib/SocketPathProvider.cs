using System;
using System.IO;

namespace APMOkLib
{
    public class SocketPathProvider : ISocketPathProvider
    {
        public string GetSocketPath() =>
             Path.Combine(Environment.GetEnvironmentVariable("ProgramData") ?? "C:\\ProgramData", "APMOk", "APMOkSvc.socket");
    }
}
