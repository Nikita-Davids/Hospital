using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class PatientsAdministration
{
    [Key]
    public int PatientsAdministration1 { get; set; }

    public string PatientId { get; set; } = null!;

    public int PatientWard { get; set; } // change to string

    public string PatientBed { get; set; } = null!; //change to int

    //public virtual Patients Patient { get; set; } = null!;
}
