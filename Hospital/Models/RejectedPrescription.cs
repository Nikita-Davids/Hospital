using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Models
{
    public class RejectedPrescription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RejectionId { get; set; }

        [Required]
        public int PrescribedID { get; set; }

        [Required]
        public int SurgeonId { get; set; }

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 1)]
        public string RejectionReason { get; set; }

        [Required]
        public DateTime RejectionDate { get; set; } = DateTime.Now; // Default value set to the current date and time

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public string PharmacistName { get; set; }

        public string PharmacistSurname { get; set; }

        [ForeignKey("SurgeonId")]
        public virtual Surgeon Surgeon { get; set; }
    }
}

