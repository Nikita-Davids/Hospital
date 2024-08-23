using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class RegisterHealthCareProfessionalViewModel
    {
        
        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string ContactNumber { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public string HealthCouncilRegistrationNumber { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
