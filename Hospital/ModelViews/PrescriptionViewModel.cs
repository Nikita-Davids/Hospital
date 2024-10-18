namespace Hospital.ModelViews
{
    public class PrescriptionViewModel
    {
        public DateTime? DispenseDateTime { get; set; }
        public string PatientIDNumber { get; set; }
        public string Patient { get; set; } // Combined property for patient name and surname
        public string ScriptBy { get; set; } // Combined property for surgeon name and surname
        public string MedicationName { get; set; }
        public int Quantity { get; set; }
        public string Dispense { get; set; }
    }
}