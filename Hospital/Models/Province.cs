using System;

using System.Collections.Generic;

namespace Hospital.Models;

public partial class Province

{

    public int ProvinceId { get; set; }

    public string? ProvinceName { get; set; }

   // public virtual ICollection<Town> Town { get; set; } = new List<Town>();

}


