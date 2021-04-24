namespace APMOk
{
    public enum BatteryFlag
    {
        None = 0,
        High = 1,
        Low = 2,
        Critical = 4,
        Charging = 8,
        NoSystemBattery = 128,
        BatteryFlagUnknown = 255,
    }
}
