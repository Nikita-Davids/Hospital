﻿using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{

    public partial class ChronicCondition
    {
        [Key]
        public int ChronicConditionId { get; set; }

        public int TreatmentCodeId { get; set; }
        [Required]
        [Display(Name = "ICD10 Code")]

        public string? Icd10Code { get; set; }

        public string? Diagnosis { get; set; }
    }
}

