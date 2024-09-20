using System;
using System.ComponentModel.DataAnnotations;

namespace Hospital
{
    public class Patients
    {
        [Key]
        [StringLength(13)]
        [Display(Name = "Patient ID")]
        public string PatientIDNumber { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string PatientName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Surname")]
        public string PatientSurname { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Address")]
        public string PatientAddress { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string PatientContactNumber { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string PatientEmailAddress { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime PatientDateOfBirth { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Gender")]
        public string PatientGender { get; set; }
    }
}

