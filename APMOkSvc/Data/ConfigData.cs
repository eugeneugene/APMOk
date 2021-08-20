using APMData;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMOkSvc.Data
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
    }
}