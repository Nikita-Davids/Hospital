using Hospital.Models;

namespace Hospital.ModelViews
{
    public class NurseDispensedAlertViewModel
    {
        public List<SurgeonPrescription> Prescriptions { get; set; }  // Change this to a list of prescriptions
        public List<AdministerMedication> AdministeredMedications { get; set; }

        public DateTime? LastAdministeredTime { get; set; }
    }
}
