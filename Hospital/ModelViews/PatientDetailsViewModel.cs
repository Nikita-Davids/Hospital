using Hospital.Models;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ModelViews
{
    public class PatientDetailsViewModel
    {
        // Patient Information
        public string PatientIDNumber { get; set; }

        [Display(Name = "First Name")]
        public string PatientName { get; set; }

        [Display(Name = "Surname")]
        public string PatientSurname { get; set; }

        [Display(Name = "Address")]
        public string PatientAddress { get; set; }

        [Display(Name = "Contact Number")]
        public string PatientContactNumber { get; set; }

        [Display(Name = "Email Address")]
        public string PatientEmailAddress { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime PatientDateOfBirth { get; set; }

        [Display(Name = "Gender")]
        public string PatientGender { get; set; }

        // Allergies
        public List<PatientAllergy> Allergies { get; set; }

        // Current Medications
        public List<PatientCurrentMedication> CurrentMedications { get; set; }

        // Medical Conditions
        public List<PatientMedicalCondition> MedicalConditions { get; set; }

        public List<PatientVital> PatientVitals { get; set; }
    }
}
