using System;
using System.Globalization;
using System.Text.Json;

namespace APMOkLib
{
    public class UpperCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name is null ? throw new ArgumentNullException(nameof(name)) : name.ToUpper(CultureInfo.CurrentCulture);
    }
}
