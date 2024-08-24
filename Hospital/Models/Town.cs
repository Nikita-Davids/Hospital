using System;
using System.Collections.Generic;

namespace Hospital.Models;

public partial class Town
{
    public int TownId { get; set; }

    public string? TownName { get; set; }

    public int? ProvinceId { get; set; }

    public virtual Province? Province { get; set; }

    public virtual ICollection<Suburb> Suburbs { get; set; } = new List<Suburb>();
}
