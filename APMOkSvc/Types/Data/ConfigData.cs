using APMOkLib;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMOkSvc.Types.Data
{
    [Table("APMCONFIG")]
    public class ConfigData : JsonToString
    {
        /// <summary>
        /// Ключевое поле
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string DeviceID { get; set; }

        [Required]
        public uint OnMains { get; set; }

        [Required]
        public uint OnBatteries { get; set; }

        public ConfigData()
        { }

        public ConfigData(string deviceID, uint onMains, uint onBatteries)
        {
            DeviceID = deviceID;
            OnMains = onMains;
            OnBatteries = onBatteries;
        }
    }
}