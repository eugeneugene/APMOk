using System.Threading;

namespace APMOk
{
    internal static class Program
    {
        public static void Main()
        {
            EventWaitHandle? @event = null;
            try
            {
                @event = new(true, EventResetMode.AutoReset, "__APMOKAPP_INSTANCE", out bool created);
                if (!created)
                    return;

                Applic app = new();
                app.InitializeComponent();
                app.Run();
            }
            finally
            {
                @event?.Dispose();
            }
        }
    }
}
