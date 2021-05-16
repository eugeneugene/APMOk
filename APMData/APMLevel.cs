using System.ComponentModel.DataAnnotations;

namespace APMData
{
    public enum APMLevel
    {
        [Display(Name = "n/a")]
        LevelNone = 0,
        [Display(Name = "Minimum with Standby enabled")]
        MinimumWithStandby = 1,
        [Display(Name = "Intermediate with Standby enabled")]
        IntermediateWithStandby = 2,
        [Display(Name = "Minimum with Standby disabled")]
        MinimumWithoutStandby = 128,
        [Display(Name = "Intermediate with Standby disabled")]
        IntermediateWithoutStandby = 129,
        [Display(Name = "Maximum")]
        Maximum = 254,
    }
}
