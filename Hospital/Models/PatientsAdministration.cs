using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class PatientsAdministration
{
    [Key]
    public int PatientsAdministration1 { get; set; }

    [Display(Name = "Patient ID")]
    [Required]
    public string PatientId { get; set; } = null!;

    [Display(Name = "Patient Ward")]
    [Required]
    public string PatientWard { get; set; } = null!;

    [Display(Name = "Patient Bed")]
    [Required]
    public int PatientBed { get; set; }

    [Display(Name = "Date Assigned")]
    [Required]
    public DateTime DateAssigned { get; set; }

    public virtual Patients? Patient { get; set; } = null!;
}
