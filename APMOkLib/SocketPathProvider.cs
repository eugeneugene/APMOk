using System.IO;

namespace APMOkLib
{
    public class SocketPathProvider : ISocketPathProvider
    {
        public string GetSocketPath() => Path.Combine(Path.GetTempPath(), "APMOkSvc.socket");
    }
}
