namespace APMData.Proto
{
    public sealed partial class PowerStateReply
    {
        public static PowerStateReply FailureReply => new ()
        {
            ACLineStatus = EACLineStatus.LineStatusUnknown,
            BatteryFlag = EBatteryFlag.BatteryFlagUnknown,
            BatteryFullLifeTime = -1,
            BatteryLifePercent = -1,
            BatteryLifeTime = -1,
        };
    }
}
