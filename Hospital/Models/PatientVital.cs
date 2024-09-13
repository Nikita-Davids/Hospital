using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class PatientVital
{
    [Key]
    public int PatientVitalId { get; set; }

    [Display(Name = "Patient ID")]
    public string PatientId { get; set; } = null!;

    public decimal? Weight { get; set; }

    public decimal? Height { get; set; }

    public decimal? Tempreture { get; set; }

    [Display(Name = "Blood Pressure")]
    public decimal? BloodPressure { get; set; }

    public decimal? Pulse { get; set; }

    public decimal? Respiratory { get; set; }

    [Display(Name = "Blood Oxygen")]
    public decimal? BloodOxygen { get; set; }

    [Display(Name = "Blood Glucose Level")]
    public decimal? BloodGlucoseLevel { get; set; }

    [Display(Name = "Vital Time")]
    public TimeSpan? VitalTime { get; set; }

   // public virtual Patients Patient { get; set; } = null!;
}
