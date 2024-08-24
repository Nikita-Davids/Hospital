using System;
using System.Collections.Generic;

namespace Hospital.Models;

public partial class Suburb
{
    public int SuburbId { get; set; }

    public string? SuburbName { get; set; }

    public string? SuburbPostalCode { get; set; }

    public int? TownId { get; set; }

    public virtual Town? Town { get; set; }
}
