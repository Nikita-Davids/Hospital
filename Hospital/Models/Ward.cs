using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.Models;

public partial class Ward
{
    [Key]
    public int WardId { get; set; }

    [Required]
    [Display(Name = "Ward Name")]

    public string WardName { get; set; } = null!;

    [Required]
    [Display(Name = "Number of Beds")]

    public int NumberOfBeds { get; set; }

    public virtual ICollection<Bed> Beds { get; set; } = new List<Bed>();
}
