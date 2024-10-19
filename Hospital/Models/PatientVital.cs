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
    public decimal? Weight { get; set; }

    [Required]
    public decimal? Height { get; set; }

    [Required]
    public decimal? Tempreture { get; set; }

    [Display(Name = "Blood Pressure")]
    public decimal? BloodPressure { get; set; }

    [Required]
    public decimal? Pulse { get; set; }

    [Required]
    public decimal? Respiratory { get; set; }

    [Display(Name = "Blood Oxygen")]
    [Required]
    public decimal? BloodOxygen { get; set; }

    [Display(Name = "Blood Glucose Level")]
    [Required]
    public decimal? BloodGlucoseLevel { get; set; }

    [Display(Name = "Vital Time")]
    [Required]
    public TimeSpan? VitalTime { get; set; }

   // public virtual Patients Patient { get; set; } = null!;
}
