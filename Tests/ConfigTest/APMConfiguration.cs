using APMOk;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigTest
{
    public interface IAPMConfiguration
    {
        string SectionName { get; }

        [JsonPropertyName("APMConfiguration")]
        IEnumerable<APMConfigurationItem> ConfigurationItems { get; }
    }

    public class APMConfiguration : JsonToString, IAPMConfiguration
    {
        public const string _sectionName = "APMConfiguration";

        public APMConfiguration()
        {
            ConfigurationItems = new List<APMConfigurationItem>();
        }

        public string SectionName { get; } = _sectionName;

        [JsonPropertyName("APMConfiguration")]
        public IEnumerable<APMConfigurationItem> ConfigurationItems { get; }
    }

    public class APMConfigurationItem
    {
        public APMConfigurationItem(string diskCaption, byte apmValue)
        {
            DiskCaption = diskCaption;
            APMValue = apmValue;
        }

        public string DiskCaption { get; set; }
        public byte APMValue { get; set; }
    }
}
