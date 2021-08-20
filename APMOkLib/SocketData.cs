using System.IO;

namespace APMOkLib
{
    public static class SocketData
    {
        public static string SocketPath => Path.Combine(Path.GetTempPath(), "APMOkSvc.socket");
    }
}
