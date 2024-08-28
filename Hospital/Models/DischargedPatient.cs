using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class DischargedPatient
{
    [Key]
    public int DischargedPatients { get; set; }

    public string? PatientId { get; set; }

    public TimeSpan DischargeTime { get; set; }

    public DateTime DischargeDate { get; set; }

    //public virtual Patients? Patient { get; set; }
}
