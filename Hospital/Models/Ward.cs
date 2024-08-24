using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class Ward
{
    [Key]
    public int WardId { get; set; }

    public string WardName { get; set; } = null!;

    public int NumberOfBeds { get; set; }

    public virtual ICollection<Bed> Beds { get; set; } = new List<Bed>();
}
