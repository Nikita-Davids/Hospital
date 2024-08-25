using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class TreatmentCode
{
    [Key]
    public int TreatmentCodeId { get; set; }
    [Required]
    [Display(Name = "ICD10 Code")]

    public string? Icd10Code { get; set; }


    [Required]
    [Display(Name = "Treatment Code Description")]

    public string? TreatmentCodeDescription { get; set; }
}
