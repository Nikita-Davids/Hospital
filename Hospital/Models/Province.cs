using System;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class Province

{

    public int ProvinceId { get; set; }
    [Required]
    [Display(Name = "Province Name")]

    public string? ProvinceName { get; set; }

   // public virtual ICollection<Town> Town { get; set; } = new List<Town>();

}


