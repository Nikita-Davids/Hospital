using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppliedProjects.ViewModels
{
    public class MedicationViewModel
    {
        [Key]
        public int MedicationId { get; set; }


        public string MedicationName { get; set; }
        public string DosageForm { get; set; }
        public string Schedule { get; set; }
        public int ReOrderLevel { get; set; }
        public List<ActiveIngredientViewModel> ActiveIngredients { get; set; }

        public MedicationViewModel()
        {
            ActiveIngredients = new List<ActiveIngredientViewModel>();
        }


        public class ActiveIngredientViewModel
        {
            [Required]
            public string ActiveIngredientName { get; set; }

        }
    }
}
