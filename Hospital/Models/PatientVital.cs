using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class PatientVital
{
    [Key]
    public int PatientVitalId { get; set; }

    [Display(Name = "Patient ID")]
    [Required]
    public string PatientId { get; set; } = null!;

    [Required]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal? Weight { get; set; }

    [Required]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal? BMI { get; set; }

    [Required]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal? Height { get; set; }

    [Required]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal? Tempreture { get; set; }

    [Display(Name = "Blood Pressure")]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal? BloodPressure { get; set; }

    [Required]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal? Pulse { get; set; }

    [Required]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal? Respiratory { get; set; }

    [Display(Name = "Blood Oxygen")]
    [Required]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal? BloodOxygen { get; set; }

    [Display(Name = "Blood Glucose Level")]
    [Required]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal? BloodGlucoseLevel { get; set; }

    [Display(Name = "Vital Time")]
    [Required]
    public TimeSpan? VitalTime { get; set; }

   // public virtual Patients Patient { get; set; } = null!;
}
