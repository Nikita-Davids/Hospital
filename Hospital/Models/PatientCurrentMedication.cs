using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class PatientCurrentMedication
{
    [Key]
    public int MedicationId { get; set; }
    [Display(Name = "Patient ID")]
    [Required]
    public string PatientId { get; set; } = null!;
    [Display(Name = "Current Medication")]
    [Required]
    public string CurrentMedication { get; set; } = null!;

    public virtual Patients Patient { get; set; }
}
