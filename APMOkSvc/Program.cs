using APMOkSvc.Code;
using System;

namespace APMOkSvc
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var manager = new StarterManager(new WindowsStarter<Startup>(), new LinuxStarter<Startup>());
                manager.Start(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
