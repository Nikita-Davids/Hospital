using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public partial class AdministerMedication
    {
        [Key]
        public int AdministerMedication_Id { get; set; }

        [Required]
        [Display(Name = "Patient ID")]
        public string Patient_Id { get; set; }

        [Required]
        [Display(Name = "Time of Administration")]
        public DateTime AdministerMedicationTime { get; set; }

        [Required]
        [Display(Name = "Script Details")]
        [StringLength(500)]
        public string ScriptDetails { get; set; }

        [Required]
        [Display(Name = "Select Medication")]
        public int MedicationId { get; set; }

        [Required]
        [Display(Name = "Quantity Administered")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Dosage Form Name")]
        public string DosageFormName { get; set; }
    }
}
