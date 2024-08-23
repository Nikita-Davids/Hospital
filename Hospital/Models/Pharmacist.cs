using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public partial class Pharmacist
    {
        [Key]
        public int PharmacistId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Surname")]
        public string Surname { get; set; }

        [Required]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [Display(Name = "Health Council Registration Number")]
        public string HealthCouncilRegistrationNumber { get; set; }
    }
}
