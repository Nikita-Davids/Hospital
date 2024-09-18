using System.Collections.Generic;
using System;

namespace Hospital.ViewModels
{
    public class PrescribedScriptViewModel
    {

        public int PrescriptionId { get; set; }
        public int PrescribedID { get; set; }//groupingID
        public int SurgeonId { get; set; }
        public string PatientIDNumber { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public string SurgeonName { get; set; }
        public string SurgeonSurname { get; set; }
        public DateTime? PrescriptionDate { get; set; }
        public List<MedicationDetailViewModel> Medications { get; set; } // List of medications
    }

    public class MedicationDetailViewModel
    {
        public string MedicationName { get; set; }
        public string PrescriptionDosageForm { get; set; }
        public string Instructions { get; set; }
        public int Quantity { get; set; }
    }
}
