using APMData;
using APMOkLib;

namespace APMOk.Code
{
    public class ApmCommandParameter : JsonEnumToString
    {
        public ApmCommandParameter(string deviceId, EPowerSource powerSource, uint apmValue)
        {
            DeviceID = deviceId;
            PowerSource = powerSource;
            ApmValue = apmValue;
        }

        public string DeviceID { get; }
        public EPowerSource PowerSource { get; }
        public uint ApmValue { get; }
    }
}
