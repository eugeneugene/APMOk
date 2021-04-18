using System.IO;

namespace APMData
{
    public static class SocketData
    {
        public static string SocketPath => Path.Combine(Path.GetTempPath(), "APMOkSvc.socket");
    }
}
