namespace APMOkLib
{
    public class SocketPathProvider : ISocketPathProvider
    {
        // public string GetSocketPath() => Path.Combine(Path.GetTempPath(), "APMOkSvc.socket");
        public string GetSocketPath() => @"C:\ProgramData\APMOk\APMOkSvc.socket";
    }
}
