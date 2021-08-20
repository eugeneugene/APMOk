using System;

namespace APMOkLib
{
    public abstract class JsonToEnumString : JsonToString
    {
        public override string ToString()
        {
            return ToString("e");
        }

        public override string ToString(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
                return base.ToString("e");
            return base.ToString(format + 'e');
        }

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrWhiteSpace(format))
                return base.ToString("e", formatProvider);
            return base.ToString(format + 'e', formatProvider);
        }

        public static new string MakeString(object ob)
        {
            return JsonToString.MakeString(ob, "e", null);
        }

        public static new string MakeString(object ob, string format, IFormatProvider formatProvider)
        {
            return JsonToString.MakeString(ob, format + "e", formatProvider);
        }
    }
}
