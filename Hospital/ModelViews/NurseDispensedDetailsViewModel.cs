using Hospital.Models;

namespace Hospital.ModelViews
{
    public class NurseDispensedDetailsViewModel
    {
        // Patient ID
        public string PatientId { get; set; }

        // List of prescriptions for the patient
        public List<SurgeonPrescription> Prescriptions { get; set; }

        // List of medications already administered
        public List<AdministerMedication> AdministeredMedications { get; set; }

        // Optional: Quantities to administer for batch processing
        public List<int> QuantitiesToAdminister { get; set; }
    }
}
