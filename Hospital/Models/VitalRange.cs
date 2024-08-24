using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class VitalRange
{
    [Key]
    public int VitalRangeId { get; set; }

    public string? VitalName { get; set; }

    public decimal? MinimumNormal { get; set; }

    public decimal? MaximumNormal { get; set; }
}
