using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigTest
{
    public interface IAPMConfiguration
    {
        string SectionName { get; }
        string DiskCaption { get; set; }
        byte APMValue { get; set; }
    }

    public class APMConfiguration : IAPMConfiguration
    {
        public const string _sectionName = "APMConfiguration";
        public string SectionName { get; } = _sectionName;
        public string DiskCaption { get; set; }
        public byte APMValue { get; set; }
    }
}
