using System.ComponentModel;
using System.Runtime.InteropServices;

namespace APMOkSvc.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public class PowerState
    {
        public ACLineStatus ACLineStatus;
        public BatteryFlag BatteryFlag;
        public byte BatteryLifePercent;
        public byte Reserved1;
        public int BatteryLifeTime;
        public int BatteryFullLifeTime;

        // direct instantation not intended, use GetPowerState.
        private PowerState() { }

        public static PowerState GetPowerState()
        {
            PowerState state = new();
            if (!GetSystemPowerStatusRef(state))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return state;
        }

        [DllImport("Kernel32", EntryPoint = "GetSystemPowerStatus")]
        private static extern bool GetSystemPowerStatusRef(PowerState sps);
    }

    // Note: Underlying type of byte to match Win32 header
    public enum ACLineStatus : byte
    {
        Offline = 0,
        Online = 1,
        Unknown = 255
    }

    public enum BatteryFlag : byte
    {
        High = 1,
        Low = 2,
        Critical = 4,
        Charging = 8,
        NoSystemBattery = 128,
        Unknown = 255
    }
}
