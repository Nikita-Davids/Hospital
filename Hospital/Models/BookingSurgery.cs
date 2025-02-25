﻿using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public class BookingSurgery
    {
        [Key]
        public int BookingSurgeryId { get; set; }


        [Required]
        [Display(Name = "Patient ID")]

        public string PatientId { get; set; }

        [Required]
        [Display(Name = "Date  of Surgery")]

        public DateTime SurgeryDate { get; set; }

        [Required]
        [Display(Name = "Time of Booking Surgery")]

        public TimeSpan SurgeryTime { get; set; }

        [Required]
        [Display(Name = "Operating Theatre Name")]

        public string? OperatingTheatreName { get; set; }

        [Required]
        [Display(Name = "Treatment Code(ICD10)")]
        public string TreatmentCodeId { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Patient Email Address")]
        public string PatientEmailAddress { get; set; }

        // Navigation Property to Patient entity
        public virtual Patients? Patient { get; set; }

    }
}

