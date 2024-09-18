using System;

namespace Hospital.ModelViews
{
    public class PrescriptionDisplayViewModel
    {
        public int PrescriptionId { get; set; }
        public string PatientIDNumber { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public DateTime? PrescriptionDate { get; set; }
        public string SurgeonName { get; set; }
        public string SurgeonSurname { get; set; }
        public string Urgent { get; set; }
       public int PrescribedID { get; set; }//groupingID

        public int MedicationCount { get; set; }
    }

}
