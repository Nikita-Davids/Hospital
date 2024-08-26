namespace Hospital.Models
{
    public partial class Restock
    {
        public int RestockId { get; set; }
        public string MedicationName { get; set; }
        public string DosageForm { get; set; }
        public int QuantityReceived { get; set; }
        public DateTime? RestockDate { get; set; }
        public int? MedicationId { get; set; }

        public virtual Medication Medication { get; set; }
    }
}
