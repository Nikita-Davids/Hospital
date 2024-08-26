using System;

namespace Hospital.ViewModels
{
    public class OrderStockViewModel
    {
        public int QuantityOrdered { get; set; }
        public DateTime OrderStockDate { get; set; }
        public int MedicationId { get; set; }
        public string MedicationName { get; set; }

        public string MedicationDisplay => $"{MedicationName} ({MedicationId})";
    }
}
