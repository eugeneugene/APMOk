using APMData;
using APMOkLib;

namespace APMOk.Code;

public class APMCommandParameter : JsonEnumToString
{
    public EPowerSource PowerSource { get; set; }
    public uint ApmValue { get; set; }
}
