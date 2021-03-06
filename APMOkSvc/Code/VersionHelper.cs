using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace APMOkSvc.Code;

public static class VersionHelper
{
    public static string? Version
    {
        get
        {
            var ass = Assembly.GetEntryAssembly();
            if (ass is null)
                return null;

            var fvi = FileVersionInfo.GetVersionInfo(ass.Location);

            StringBuilder version = new();
            version.AppendLine(VersionLineInt(fvi));
            if (!string.IsNullOrWhiteSpace(fvi.Comments))
                version.AppendLine(fvi.Comments);
            if (!string.IsNullOrWhiteSpace(fvi.LegalCopyright))
                version.AppendLine(fvi.LegalCopyright);
            return version.ToString();
        }
    }

    public static string? VersionLine
    {
        get
        {
            var ass = Assembly.GetEntryAssembly();
            if (ass is null)
                return null;

            var fvi = FileVersionInfo.GetVersionInfo(ass.Location);

            return VersionLineInt(fvi);
        }
    }

    public static IEnumerable<string>? VersionLines
    {
        get
        {
            var ass = Assembly.GetEntryAssembly();
            if (ass is null)
                return null;

            var fvi = FileVersionInfo.GetVersionInfo(ass.Location);

            var lines = new List<string> { VersionLineInt(fvi), };
            if (!string.IsNullOrWhiteSpace(fvi.Comments))
                lines.Add(fvi.Comments);
            if (!string.IsNullOrWhiteSpace(fvi.LegalCopyright))
                lines.Add(fvi.LegalCopyright);
            return lines;
        }
    }

    private static string VersionLineInt(FileVersionInfo fvi) => $"{fvi.ProductName} {fvi.ProductVersion}, {fvi.FileVersion}";
}
