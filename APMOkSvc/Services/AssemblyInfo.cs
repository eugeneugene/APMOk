using System;
using System.Reflection;

namespace APMOkSvc.Services;

/// <summary>
/// EntryAssembly Information helper service
/// DI Lifetime: Transient
/// </summary>
public class AssemblyInfo
{
    private readonly Assembly _assembly;

    public AssemblyInfo()
    {
        _assembly = Assembly.GetEntryAssembly() ?? throw new SystemException("Cannot get entry assembly");
    }

    public AssemblyInfo(Assembly assembly)
    {
        _assembly = assembly;
    }

    public Assembly Assembly => _assembly;

    public AssemblyHashAlgorithm? AlgorithmId
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyAlgorithmIdAttribute>();
            if (attr is not null)
                return (AssemblyHashAlgorithm)attr.AlgorithmId;
            return null;
        }
    }

    public string? Company
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (attr is not null)
                return attr.Company;
            return null;
        }
    }

    public string? Copyright
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            if (attr is not null)
                return attr.Copyright;
            return null;
        }
    }

    public string? Configuration
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
            if (attr is not null)
                return attr.Configuration;
            return null;
        }
    }

    public string? Culture
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyCultureAttribute>();
            if (attr is not null)
                return attr.Culture;
            return null;
        }
    }

    public string? DefaultAlias
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyDefaultAliasAttribute>();
            if (attr is not null)
                return attr.DefaultAlias;
            return null;
        }
    }

    public bool? DelaySign
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyDelaySignAttribute>();
            if (attr is not null)
                return attr.DelaySign;
            return null;
        }
    }

    public string? Description
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            if (attr is not null)
                return attr.Description;
            return null;
        }
    }

    public Version? FileVersion
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            if (attr is not null)
            {
                Version version = new(attr.Version);
                return version;
            }
            return null;
        }
    }

    public AssemblyNameFlags? Flags
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyFlagsAttribute>();
            if (attr is not null)
                return (AssemblyNameFlags)attr.AssemblyFlags;
            return null;
        }
    }

    public Version? InformationalVersion
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attr is not null)
            {
                Version version = new(attr.InformationalVersion);
                return version;
            }
            return null;
        }
    }

    public string? KeyFile
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyKeyFileAttribute>();
            if (attr is not null)
                return attr.KeyFile;
            return null;
        }
    }

    public string? KeyName
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyKeyNameAttribute>();
            if (attr is not null)
                return attr.KeyName;
            return null;
        }
    }

    public (string, string?) Metadata
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyMetadataAttribute>();
            if (attr is not null)
                return (attr.Key, attr.Value);
            return default;
        }
    }

    public string? Product
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyProductAttribute>();
            if (attr is not null)
                return attr.Product;
            return null;
        }
    }

    public (string, string) SignatureKey
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblySignatureKeyAttribute>();
            if (attr is not null)
                return (attr.Countersignature, attr.PublicKey);
            return default;
        }
    }

    public string? Title
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            if (attr is not null)
                return attr.Title;
            return null;
        }
    }

    public string? Trademark
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyTrademarkAttribute>();
            if (attr is not null)
                return attr.Trademark;
            return null;
        }
    }

    public Version? Version
    {
        get
        {
            var attr = _assembly.GetCustomAttribute<AssemblyVersionAttribute>();
            if (attr is not null)
                return new(attr.Version);
            return null;
        }
    }
}
