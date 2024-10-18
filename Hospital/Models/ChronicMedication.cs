using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public class ChronicMedication
    {      
            [Key]
            public int ChronicMedicationId { get; set; }


            [Required]
            [Display(Name = "Chronic Medication Name")]
            public string CMedicationName { get; set; }


            [Required]
            [Display(Name = "Dosage Form")]
            public string CDosageForm { get; set; }


            [Required]
            [Display(Name = "Schedule")]
            public string CSchedule { get; set; }


            [Required]
            [Display(Name = "Medication Active Ingredients")]
            public string CMedicationActiveIngredients { get; set; }
           
        }
}
