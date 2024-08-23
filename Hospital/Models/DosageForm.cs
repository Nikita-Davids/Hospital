using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public class DosageForm
    {

        [Key]
        public int DosageFormID { get; set; }

        [Required]
        [Display(Name = "Dosage Form Name")]
        public string DosageFormName { get; set; }
    }
}
