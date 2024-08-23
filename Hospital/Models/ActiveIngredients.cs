using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public class ActiveIngredients
    {
        [Key]
        // Property representing fields in the apprAllocateGoods table in the database
        public int ActiveIngredientID { get; set; }
       public string ActiveIngredientName { get; set; }
    }
}

