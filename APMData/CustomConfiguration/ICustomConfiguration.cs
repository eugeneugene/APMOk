using Microsoft.Extensions.Configuration;

namespace CustomConfiguration
{
    public interface ICustomConfiguration
    {
        string SectionName { get; }

        IConfiguration Configuration { get; }
    }
}
