using APMOkSvc.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace APMOkSvc.Code;

public static class AssemblyInfoExtensions
{
    public static string VersionString(this AssemblyInfo assemblyInfo)
    {
        var assemblyName = assemblyInfo.Assembly.GetName();
        var version = assemblyName.Version;

        StringBuilder sb = new();

        var title = assemblyInfo.Title;
        var configuration = assemblyInfo.Configuration;
        var fileVersion = assemblyInfo.FileVersion;

        if (title is not null)
            sb.Append(title);
        if (sb.Length > 0)
            sb.Append(' ');
        if (configuration is not null)
            sb.Append(configuration);
        if (sb.Length > 0)
            sb.Append(' ');
        if (fileVersion is not null)
            sb.Append(fileVersion);
        if (version is not null && version.Build != 0 && version.Revision != 0)
        {
            if (sb.Length > 0)
                sb.Append(' ');
            DateTime build = BuildDate(version);
            sb.Append(build.ToString("s", CultureInfo.CurrentCulture));
        }

        return sb.ToString();
    }

    public static string FullVersionInfo(this AssemblyInfo assemblyInfo)
    {
        StringBuilder version = new();
        version.AppendLine(assemblyInfo.VersionString());
        var description = assemblyInfo.Description;
        if (description is not null)
            version.AppendLine(description);
        var copyright = assemblyInfo.Copyright;
        if (copyright is not null)
            version.AppendLine(copyright);

        return version.ToString();
    }

    public static IEnumerable<string> FullVersionStrings(this AssemblyInfo assemblyInfo)
    {
        List<string> strings = new()
        {
            assemblyInfo.VersionString()
        };
        var description = assemblyInfo.Description;
        if (description is not null)
            strings.Add(description);
        var copyright = assemblyInfo.Copyright;
        if (copyright is not null)
            strings.Add(copyright);

        return strings;
    }

    private static DateTime BuildDate(Version version)
    {
        DateTime build = new(2000, 1, 1);
        build = build.AddDays(version.Build);
        build = build.AddSeconds(version.Revision * 2.0);
        return build;
    }
}
