using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class Bed
{
    [Key]
    public int BedId { get; set; }

    public int WardId { get; set; }

    public int BedNumber { get; set; }

    public bool? IsOccupied { get; set; }

   // public virtual ICollection<PatientWardAssignment> PatientWardAssignments { get; set; } = new List<PatientWardAssignment>();

   // public virtual Ward Ward { get; set; } = null!;
}
