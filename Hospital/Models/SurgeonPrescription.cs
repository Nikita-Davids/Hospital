using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public partial class SurgeonPrescription
    {
        [Key]
        public int PrescriptionId { get; set; }
        public string PatientIdnumber { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public int MedicationId { get; set; }
        public string MedicationName { get; set; }
        public string PrescriptionDosageForm { get; set; }
        public int SurgeonId { get; set; }
        public int Quantity { get; set; }
        public string Instructions { get; set; }
        public string Urgent { get; set; }
        public string Dispense { get; set; }
        public DateTime? PrescriptionDate { get; set; }
        public DateTime? DispenseDateTime { get; set; }

        public string PharmacistName { get; set; }

        public string PharmacistSurname { get; set; }

        public virtual Medication Medication { get; set; }
        public virtual Surgeon Surgeon { get; set; }
    }
}
