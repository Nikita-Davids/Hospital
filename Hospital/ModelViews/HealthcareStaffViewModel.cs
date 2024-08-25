using Hospital.Models;
using System.Collections.Generic;
namespace Hospital.ViewModels


{
    public class HealthcareStaffViewModel
    {
        public List<Nurse> Nurses { get; set; }
        public List<Pharmacist> Pharmacists { get; set; }
        public List<Surgeon> Surgeons{ get; set; }
    }
}
