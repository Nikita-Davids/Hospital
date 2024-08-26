using System;

namespace Hospital.ViewModels
{
    public class StockOrderViewModel
    {
        public int MedicationId { get; set; }
        public string? MedicationName { get; set; }
        public string? DosageForm { get; set; }
        public int StockOnHand { get; set; }
        public int ReOrderLevel { get; set; }
        public bool IsSelected { get; set; }
        public int QuantityToOrder { get; set; }
        public DateTime OrderStockDate { get; set; }
    }
}
