using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class Bed
{
    [Key]
    public int BedId { get; set; }

    [Required]
    [Display(Name = "Ward Name")]

    public int WardId { get; set; }

    [Required]
    [Display(Name = "Bed Number")]

    public int BedNumber { get; set; }

    [Required]
    [Display(Name = "Bed Occupied")]

    public bool? IsOccupied { get; set; }

   // public virtual ICollection<PatientWardAssignment> PatientWardAssignments { get; set; } = new List<PatientWardAssignment>();

   // public virtual Ward Ward { get; set; } = null!;
}
