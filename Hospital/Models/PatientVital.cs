using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public partial class PatientVital
    {
        [Key]
        public int PatientVitalId { get; set; }

        [Display(Name = "Patient ID")]
        [Required(ErrorMessage = "Patient ID is required.")]
        public string PatientId { get; set; } = null!;

        [Required(ErrorMessage = "Weight is required.")]
        [Range(1, 500, ErrorMessage = "Weight must be between 1 and 500 kg.")]
        public decimal? Weight { get; set; }

        [Required(ErrorMessage = "Height is required.")]
        [Range(0.1, 3.0, ErrorMessage = "Height must be between 0.1 and 3.0 meters.")]
        public decimal? Height { get; set; }

        [Required(ErrorMessage = "BMI is required.")]
        [Range(10, 100, ErrorMessage = "BMI must be between 10 and 100.")]
        public decimal? BMI { get; set; }

        [Required(ErrorMessage = "Temperature is required.")]
        [Range(15, 70, ErrorMessage = "Temperature must be between 15°C and 70°C.")]
        public decimal? Tempreture { get; set; }

        [Required(ErrorMessage = "Blood Pressure is required.")]
        [RegularExpression(@"^\d{1,4}/\d{1,4}$", ErrorMessage = "Blood pressure must be in the format '120/80'.")]
        public string? BloodPressure { get; set; }

        [Required(ErrorMessage = "Pulse rate is required.")]
        [Display(Name = "Heart Rate")]
        [Range(1, 400, ErrorMessage = "Pulse rate must be between 1 and 400 bpm.")]
        public decimal? Pulse { get; set; }

        [Required(ErrorMessage = "Respiratory rate is required.")]
        [Range(1, 100, ErrorMessage = "Respiratory rate must be between 1 and 100 breaths per minute.")]
        public decimal? Respiratory { get; set; }

        [Required(ErrorMessage = "Oxygen Saturation is required.")]
        [Range(0, 100, ErrorMessage = "Oxygen saturation must be between 0% and 100%.")]
        public decimal? BloodOxygen { get; set; }

        [Required(ErrorMessage = "Blood glucose level is required.")]
        [Range(0, 50, ErrorMessage = "Blood glucose level must be between 0 and 50 mmol/L.")]
        public decimal? BloodGlucoseLevel { get; set; }

        [Required(ErrorMessage = "Vital time is required.")]
        public TimeSpan? VitalTime { get; set; }

        // Helper method for blood pressure parsing
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
            return (null, null);
        }

        public virtual Patients? Patient { get; set; } = null!;
    }
}
