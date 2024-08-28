using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class PatientWardAssignment
{
    [Key]
    public int AssignmentId { get; set; }

    public string PatientId { get; set; } = null!;

    public int BedId { get; set; }

    public DateTime DateAssigned { get; set; }

    public DateTime? DateDischarged { get; set; }

    //public virtual Bed Bed { get; set; } = null!;

    //public virtual Patients Patient { get; set; } = null!;
}
