using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMOkLib;

public enum APMLevel
{
    [Display(Name = "n/a")]
    [NotMapped]
    LevelNone = 0,
    [Display(Name = "Minimum power consumption with Standby")]
    MinimumWithStandby = 1,
    [Display(Name = "Intermediate power management with Standby")]
    IntermediateWithStandby = 2,
    [Display(Name = "Minimum power consumption without Standby")]
    MinimumWithoutStandby = 128,
    [Display(Name = "Intermediate power management without Standby")]
    IntermediateWithoutStandby = 129,
    [Display(Name = "Maximum performance")]
    Maximum = 254,
}
