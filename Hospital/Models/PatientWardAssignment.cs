using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class PatientWardAssignment
{
    [Key]
    public int AssignmentId { get; set; }

    [Required]
    public string PatientId { get; set; } = null!;

    [Required]
    public int BedId { get; set; }

    [Required]
    public DateTime DateAssigned { get; set; }

    [Required]
    public DateTime? DateDischarged { get; set; }

    //public virtual Bed Bed { get; set; } = null!;

    //public virtual Patients Patient { get; set; } = null!;
}
