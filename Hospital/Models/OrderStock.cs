using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public partial class OrderStock
    {
        [Key]
        public int OrderStockId { get; set; }
        public int MedicationId { get; set; }
        public int QuantityOrdered { get; set; }
        public DateTime OrderStockDate { get; set; }

        public virtual Medication Medication { get; set; }
    }
}
