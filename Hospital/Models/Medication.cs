﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Hospital.Models
{
    public partial class Medication
    {
        public Medication()
        {
            OrderStocks = new HashSet<OrderStock>();
            Restocks = new HashSet<Restock>();
            Stocks = new HashSet<Stock>();
            SurgeonPrescriptions = new HashSet<SurgeonPrescription>();
        }
        [Key]
        public int MedicationId { get; set; }


        [Required]
        [Display(Name = "Medication Name")]
        public string MedicationName { get; set; }


        [Required]
        [Display(Name = "Dosage Form")]
        public string DosageForm { get; set; }


        [Required]
        [Display(Name = "Schedule")]
        public string Schedule { get; set; }


        [Required]
        [Range(1, 10000, ErrorMessage = "ReOrder Level must be between 1 and 10,000.")]
        [RegularExpression(@"^[1-9][0-9]{0,4}$", ErrorMessage = "ReOrder Level cannot be negative or contain symbols .")]
        [Display(Name = "Reorder Level")]
        public int ReOrderLevel { get; set; }


        [Required]
        [Display(Name = "Medication Active Ingredients")]
        public string MedicationActiveIngredients { get; set; }
        public string IsDeleted { get; set; }

        public virtual ICollection<OrderStock> OrderStocks { get; set; }
        public virtual ICollection<Restock> Restocks { get; set; }
        public virtual ICollection<Stock> Stocks { get; set; }
        public virtual ICollection<SurgeonPrescription> SurgeonPrescriptions { get; set; }


    }


}
