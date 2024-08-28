namespace Hospital.ModelViews
{
    public class RejectedPrescriptionViewModel
    {
        public int PrescriptionId { get; set; }
        public string PatientIDNumber { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public string MedicationName { get; set; }
        public string PrescriptionDosageForm { get; set; }
        public int Quantity { get; set; }
        public string Instructions { get; set; }
        public string Urgent { get; set; }
        public int SurgeonID { get; set; }
        
        public DateTime? PrescriptionDate { get; set; }
        public DateTime? DispenseDateTime { get; set; }
        public string PharmacistName { get; set; }
        public string PharmacistSurname { get; set; }
        public string RejectionReason { get; set; }
        public DateTime RejectionDate { get; set; }
        public string Status { get; set; }
    }

}
