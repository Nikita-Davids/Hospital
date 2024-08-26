using System;

namespace Hospital.ModelViews
{
    public class PrescriptionDisplayViewModel
    {
        public string PatientIDNumber { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public DateTime? PrescriptionDate { get; set; }
        public string SurgeonName { get; set; }
        public string SurgeonSurname { get; set; }
        public string Urgent { get; set; }
    }

}
