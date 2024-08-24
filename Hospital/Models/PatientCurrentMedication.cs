using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class PatientCurrentMedication
{
    [Key]
    public int MedicationId { get; set; }

    public string PatientId { get; set; } = null!;

    public string CurrentMedication { get; set; } = null!;

    public virtual Patients Patient { get; set; } = null!;
}
