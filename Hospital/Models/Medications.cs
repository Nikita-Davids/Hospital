using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public class Medications
    {
        [Key]
        // Property representing fields in the apprAllocateGoods table in the database
         public int MedicationID { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string MedicationName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string DosageForm { get; set; }
        [Required(ErrorMessage = "This field is required")]

        public string Schedule { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public int ReOrderLevel { get; set; }
    }
}
   