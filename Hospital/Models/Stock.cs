﻿using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public partial class Stock
    {
        [Key]
        public int StockId { get; set; }
        public int MedicationId { get; set; }
        public int StockOnHand { get; set; }

        public virtual Medication Medication { get; set; }
    }
}
