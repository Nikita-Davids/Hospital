using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class PatientVital
{
    [Key]
    public int PatientVitalId { get; set; }

    public string PatientId { get; set; } = null!;

    public decimal? Weight { get; set; }

    public decimal? Height { get; set; }

    public decimal? Tempreture { get; set; }

    public decimal? BloodPressure { get; set; }

    public decimal? Pulse { get; set; }

    public decimal? Respiratory { get; set; }

    public decimal? BloodOxygen { get; set; }

    public decimal? BloodGlucoseLevel { get; set; }

    public TimeSpan? VitalTime { get; set; }

    public virtual Patients Patient { get; set; } = null!;
}
