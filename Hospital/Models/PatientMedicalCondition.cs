using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class PatientMedicalCondition
{
    [Key]
    public int ConditionId { get; set; }

    [Display(Name = "Patient ID")]
    public string PatientId { get; set; } = null!;

    [Display(Name = "Medical Condition")]
    public string MedicalCondition { get; set; } = null!;

    //public virtual Patients Patient { get; set; } = null!;
}
