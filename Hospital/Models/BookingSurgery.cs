using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    public class BookingSurgery
    {
            [Key]
            public int BookingSurgeryId { get; set; }

            public string PatientId { get; set; }

            public DateTime SurgeryDate { get; set; }

            public TimeSpan SurgeryTime { get; set; }

            public int TreatmentCodeId { get; set; }

            public string PatientEmailAddress { get; set; }
        
    }
}

