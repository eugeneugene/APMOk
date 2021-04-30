﻿using System;
using System.Globalization;
using System.Text.Json;

namespace APMData
{
    public class UpperCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name == null ? throw new ArgumentNullException(nameof(name)) : name.ToUpper(CultureInfo.CurrentCulture);
    }
}
