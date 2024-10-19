using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class PatientAllergy
{
    [Key]
    public int AllergyId { get; set; }
    [Required]
    public string PatientId { get; set; }
    [Required]
    public string Allergy { get; set; } 
    //public virtual Patients Patient { get; set; }
}
