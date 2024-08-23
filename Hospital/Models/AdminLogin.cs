using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public class AdminLogin
    {
        [Key]

          [Required]
            [Display(Name = "Email Address")]
            public string EmailAddress { get; set; }
        }
    }


