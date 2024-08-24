using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public partial class DayHospital
    {
        [Key]
        public int HospitalId { get; set; }

        [Required]
        [Display(Name = "Hospital Name")]

        public string HospitalName { get; set; } = null!;
        [Required]

        public string Address { get; set; } = null!;
         [Required]

        public string City { get; set; } = null!;

        public string Province { get; set; } = null!;

        [Required]
        [Display(Name = "Postal Code")]

        public string PostalCode { get; set; } = null!;
        [Required]
        [Display(Name = "Contact Number")]

        public string ContactNumber { get; set; } = null!;
        [Required]
        [Display(Name = "Email Address")]

        public string EmailAddress { get; set; } = null!;
        [Required]
        [Display(Name = "Practice Manager")]

        public string PracticeManager { get; set; } = null!;
        [Required]
        [Display(Name = "Purchase Manager Email")]

        public string PurchaseManagerEmail { get; set; } = null!;
    }
}