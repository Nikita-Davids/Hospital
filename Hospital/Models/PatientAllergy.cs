using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class PatientAllergy
{
    [Key]
    public int AllergyId { get; set; }

    public string PatientId { get; set; }

    public string Allergy { get; set; } 
    public virtual Patients Patient { get; set; }
}
