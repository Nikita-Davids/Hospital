using Hospital.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class PatientOverviewViewModel
    {
        // Patient Details
        public string PatientIDNumber { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public string PatientAddress { get; set; }
        public string PatientContactNumber { get; set; }
        public string PatientEmailAddress { get; set; }
        public DateTime PatientDateOfBirth { get; set; }
        public string PatientGender { get; set; }

        // Vital Signs
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? BMI { get; set; }
        public decimal? Temperature { get; set; }
        //public decimal? BloodPressure { get; set; }
        public string? BloodPressure { get; set; }

        public decimal? Pulse { get; set; }
        public decimal? Respiratory { get; set; }
        public decimal? BloodOxygen { get; set; }
        public decimal? BloodGlucoseLevel { get; set; }
        public TimeSpan? VitalTime { get; set; }

        // Allergies
        public List<PatientAllergy> Allergies { get; set; } = new List<PatientAllergy>();

        // Current Medications
        public List<PatientCurrentMedication> CurrentMedications { get; set; } = new List<PatientCurrentMedication>();

        // Medical Conditions
        public List<PatientMedicalCondition> MedicalConditions { get; set; } = new List<PatientMedicalCondition>();

        //Medication administered
        public List<AdministerMedication> AdministeredMedications { get; set; }
    }
}
