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
        public string Caption { get; set; }
        [Required]
        public uint DefaultValue { get; set; }
        [Required]
        public uint CurrentValue { get; set; }

        public ConfigData()
        { }
    }
}