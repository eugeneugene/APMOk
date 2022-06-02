using Microsoft.Extensions.Configuration;

namespace APMOkLib.CustomConfiguration;

public interface ISmartConfiguration
{
    string SectionName { get; }
    IConfiguration Configuration { get; }
}
