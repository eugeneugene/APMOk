using Microsoft.Extensions.Configuration;

namespace APMOkLib.CustomConfiguration
{
    public abstract class SmartConfiguration : ISmartConfiguration
    {
        public SmartConfiguration(string sectionName, IConfiguration configuration)
        {
            SectionName = sectionName;
            Configuration = configuration;
        }

        public string SectionName { get; }

        public IConfiguration Configuration { get; }
    }
}
