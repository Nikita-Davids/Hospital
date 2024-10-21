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

    //[Display(Name = "Blood Pressure")]
    //[DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    //public decimal? BloodPressure { get; set; }

    [Display(Name = "Blood Pressure")]
    [Required]
    [RegularExpression(@"^\d{1,3}/\d{1,3}$", ErrorMessage = "Blood pressure must be in the format '120/80'.")]
    public string? BloodPressure { get; set; }

    [Required]
    [Display(Name = "Heart Rate")]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal? Pulse { get; set; }

    [Required]
    [Display(Name = "Respiratory Rate")]
    [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
    public decimal? Respiratory { get; set; }

    [Display(Name = "Oxygen Saturation")]
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

    // Add validation logic to ensure blood pressure is in the correct format
    public (int? Systolic, int? Diastolic) GetBloodPressureValues()
    {
        if (BloodPressure != null && BloodPressure.Contains("/"))
        {
            var parts = BloodPressure.Split('/');
            if (int.TryParse(parts[0], out int systolic) && int.TryParse(parts[1], out int diastolic))
            {
                return (systolic, diastolic);
            }
        }
        return (null, null); // Return null if invalid
    }

    // public virtual Patients Patient { get; set; } = null!;
}
