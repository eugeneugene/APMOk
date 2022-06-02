using System;

namespace APMOkLib;

public abstract class JsonEnumToString : JsonToString
{
    public override string ToString()
    {
        return ToString("E");
    }

    public override string ToString(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
            return base.ToString("E");
        return base.ToString(format);
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrWhiteSpace(format))
            return base.ToString("E", formatProvider);
        return base.ToString(format, formatProvider);
    }

    public static new string MakeString(object ob)
    {
        return JsonToString.MakeString(ob, "E", null);
    }

    public static new string MakeString(object ob, string format, IFormatProvider formatProvider)
    {
        return JsonToString.MakeString(ob, format, formatProvider);
    }
}
