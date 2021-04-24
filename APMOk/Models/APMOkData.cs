using System;

namespace APMOk
{
    public class APMOkData : JsonToString
    {
        public PowerState PowerState { get; set; }
    }

    public class PowerState : JsonToEnumString, IEquatable<PowerState>
    {
        public ACLineStatus ACLineStatus { get; set; }
        public BatteryFlag BatteryFlag { get; set; }
        public int BatteryLifePercent { get; set; }
        public int BatteryLifeTime { get; set; }
        public int BatteryFullLifeTime { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as PowerState);
        }

        public bool Equals(PowerState other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (ACLineStatus != other.ACLineStatus) return false;
            if (BatteryFlag != other.BatteryFlag) return false;
            if (BatteryLifePercent != other.BatteryLifePercent) return false;
            if (BatteryLifeTime != other.BatteryLifeTime) return false;
            if (BatteryFullLifeTime != other.BatteryFullLifeTime) return false;
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 1;
            if (ACLineStatus != ACLineStatus.Offline) hash ^= ACLineStatus.GetHashCode();
            if (BatteryFlag != BatteryFlag.None) hash ^= BatteryFlag.GetHashCode();
            if (BatteryLifePercent != 0) hash ^= BatteryLifePercent.GetHashCode();
            if (BatteryLifeTime != 0) hash ^= BatteryLifeTime.GetHashCode();
            if (BatteryFullLifeTime != 0) hash ^= BatteryFullLifeTime.GetHashCode();
            return hash;
        }
    }
}
