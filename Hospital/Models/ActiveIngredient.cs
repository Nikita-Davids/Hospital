using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Hospital.Models
{
    public  class ActiveIngredient
    {
        [Key]
        public int IngredientId { get; set; }

        [Required]
        [Display(Name = "Ingredient Name")]
        public string IngredientName { get; set; }

        [Required]
        [Display(Name = "Strength")]
        public string Strength { get; set; }
    }
}
