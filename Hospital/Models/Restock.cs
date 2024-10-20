using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public partial class Restock
    {
        [Key]
        public int RestockId { get; set; }
        [Required]
        [Display(Name = "Medication Name")]
        public string MedicationName { get; set; }
        [Required]
        [Display(Name = "Dosage Form")]
        public string DosageForm { get; set; }
        [Required]

        [Display(Name = "Quantity Received")]
        public int QuantityReceived { get; set; }
        [Required]
        [Display(Name = "Restock Date")]
        public DateTime? RestockDate { get; set; }
        public int? MedicationId { get; set; }

        public virtual Medication Medication { get; set; }
    }


}
