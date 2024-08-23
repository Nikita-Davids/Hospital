using System.ComponentModel.DataAnnotations;

    namespace Hospital.Models
    {
        public class Nurse
        {
        [Key]
        public int NurseID { get; set; }

            [Required]
            [StringLength(50)]
            public string Name { get; set; }

            [Required]
            [StringLength(50)]
            public string Surname { get; set; }

            [Required]
            [StringLength(20)]
            public string ContactNumber { get; set; }

            [Required]
            [StringLength(100)]
            public string EmailAddress { get; set; }

            [Required]
            [StringLength(20)]
            public string HealthCouncilRegistrationNumber { get; set; }
        }
    }
