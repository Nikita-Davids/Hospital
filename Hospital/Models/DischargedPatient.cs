using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class DischargedPatient
{
    [Key]
    public int DischargedPatients { get; set; }

    [Display(Name = "Patient ID")]
    public string? PatientId { get; set; }

    [Display(Name = "Discharge Time")]
    public TimeSpan DischargeTime { get; set; }

    [Display(Name = "Discharge Date")]
    public DateTime DischargeDate { get; set; }

    //public virtual Patients? Patient { get; set; }
}
