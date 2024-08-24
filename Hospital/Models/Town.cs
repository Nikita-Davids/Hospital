using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class Town
{
    public int TownId { get; set; }

    [Required]
    [Display(Name = "Town Name")]
    public string? TownName { get; set; }

    [Required]
    [Display(Name = "Province Name")]

    public int? ProvinceId { get; set; }

    public virtual Province? Province { get; set; }

    public virtual ICollection<Suburb> Suburbs { get; set; } = new List<Suburb>();
}
