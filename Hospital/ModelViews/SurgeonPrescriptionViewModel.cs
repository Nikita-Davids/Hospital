using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hospital.ViewModels
{
    public class SurgeonPrescriptionViewModel
    {
        [Required(ErrorMessage = "Patient ID Number is required.")]
        [Display(Name = "Patient ID Number")]
        public string PatientIDNumber { get; set; }

        [Required(ErrorMessage = "Patient Name is required.")]
        [Display(Name = "Patient Name")]
        public string PatientName { get; set; }

        [Required(ErrorMessage = "Patient Surname is required.")]
        [Display(Name = "Patient Surname")]
        public string PatientSurname { get; set; }

        [Required(ErrorMessage = "At least one medication is required.")]
        public List<MedicationPrescription> Medications { get; set; } = new List<MedicationPrescription>();

        [Required(ErrorMessage = "Surgeon is required.")]
        [Display(Name = "Surgeon Name And Surname")]
        public int SurgeonID { get; set; }

        [Required(ErrorMessage = "Urgency status is required.")]
        [Display(Name = "Urgent")]
        public string Urgent { get; set; }

        [Required(ErrorMessage = "Prescription Date is required.")]
        [Display(Name = "Prescription Date")]
        public DateTime PrescriptionDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Dispense status is required.")]
        [Display(Name = "Dispense")]
        public string Dispense { get; set; } = "Not Dispense"; // Default value

        [Required(ErrorMessage = "Pharmacist Name status is required.")]
        [Display(Name = "Pharmacist Name")]
        public string PharmacistName { get; set; } = "PharmacistName"; // Default value

        [Required(ErrorMessage = "Pharmacist Surname status is required.")]
        [Display(Name = "Pharmacist Surname")]
        public string PharmacistSurname { get; set; } = "PharmacistSurname"; // Default value

        public DateTime? DispenseDateTime { get; set; }
    }

    public class MedicationPrescription
    {
        [Required(ErrorMessage = "Medication Name is required.")]
        [Display(Name = "Medication Name")]
        public string MedicationName { get; set; }

        [Required(ErrorMessage = "Dosage Form is required.")]
        [Display(Name = "Dosage Forme")]
        public string PrescriptionDosageForm { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Instructions are required.")]
        [Display(Name = "Instructions")]
        public string Instructions { get; set; }
    }
}
