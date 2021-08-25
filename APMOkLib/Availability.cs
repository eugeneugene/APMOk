using Ardalis.SmartEnum;

namespace APMOkLib
{
    public sealed class Availability : SmartEnum<Availability, ushort>
    {
        public static readonly Availability Ok = new("Ok", 0);
        public static readonly Availability Other = new("Other", 1);
        public static readonly Availability Unknown = new("Unknown", 2);
        public static readonly Availability Running = new("Running/Full Power", 3);
        public static readonly Availability Warning = new("Warning", 4);
        public static readonly Availability InTest = new("In Test", 5);
        public static readonly Availability NotApplicable = new("Not Applicable", 6);
        public static readonly Availability PowerOff = new("Power Off", 7);
        public static readonly Availability OffLine = new("Off Line", 8);
        public static readonly Availability OffDuty = new("Off Duty", 9);
        public static readonly Availability Degraded = new("Degraded", 10);
        public static readonly Availability NotInstalled = new("Not Installed", 11);
        public static readonly Availability InstallError = new("Install Error", 12);
        public static readonly Availability PowerSaveUnknown = new("Power Save - Unknown", 13);
        public static readonly Availability PowerSaveLowPowerMode = new("Power Save - Low Power Mode", 14);
        public static readonly Availability PowerSaveStandby = new("Power Save - Standby", 15);
        public static readonly Availability PowerCycle = new("Power Cycle", 16);
        public static readonly Availability PowerSaveWarning = new("Power Save - Warning", 17);
        public static readonly Availability Paused = new("Paused", 18);
        public static readonly Availability NotReady = new("Not Ready", 19);
        public static readonly Availability NotConfigured = new("Not Configured", 20);
        public static readonly Availability Quiesced = new("Quiesced ", 21);

        private Availability(string name, ushort type) : base(name, type)
        { }
    }
}
