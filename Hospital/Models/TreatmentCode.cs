using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class TreatmentCode
{
    [Key]
    public int TreatmentCodeId { get; set; }

    public string? Icd10Code { get; set; }

    public string? TreatmentCodeDescription { get; set; }
}
