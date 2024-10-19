using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "Date  of Suregry")]

        public DateTime SurgeryDate { get; set; }

        [Required]
        [Display(Name = "Time of Booking Suregry")]

        public TimeSpan SurgeryTime { get; set; }

        [Required]
        [Display(Name = "Treatment Code(ICD10)")]
        public int TreatmentCodeId { get; set; }

        [Required]
        [Display(Name = "Patient Email Address")]
        public string PatientEmailAddress { get; set; }
        
    }
}

