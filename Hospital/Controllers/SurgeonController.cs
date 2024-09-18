using Hospital.Data;
using Hospital.Models;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hospital.Controllers
{
    public class SurgeonController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor to inject the context
        public SurgeonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET method to load the SurgeonPrescription view with dropdowns for patients, medications, dosage forms, and surgeons
        public IActionResult AddSurgeonPrescription()
        {
            // Retrieve a list of patients to populate the patient dropdown
            var patients = _context.Patients.Select(p => new SelectListItem
            {
                // Display name and ID number of each patient
                Text = $"{p.PatientName}, {p.PatientSurname} ({p.PatientIDNumber})",
                Value = p.PatientIDNumber
            }).ToList();

            // Retrieve a list of medications to populate the medication dropdown
            var medications = _context.Medication
                .Where(m => m.IsDeleted != "Deleted")
                .OrderBy(m => m.MedicationName) // Sort by MedicationName A-Z
                .Select(m => new SelectListItem
                {
                    // Display medication name
                    Text = m.MedicationName,
                    Value = m.MedicationName
                })
                .Distinct()
                .ToList();

            // Retrieve distinct dosage forms for populating the dosage form dropdown
            var dosageForms = _context.Medication
                .OrderBy(m => m.DosageForm) // Sort by DosageForm A-Z
                .Select(m => m.DosageForm)
                .Distinct()
                .ToList();

            // Retrieve a list of surgeons to populate the surgeon dropdown
            ViewBag.Patients = patients;
            ViewBag.Medications = medications;
            ViewBag.DosageForms = dosageForms;
            ViewBag.Surgeons = _context.Surgeons
                .Select(s => new SelectListItem
                {
                    // Display surgeon name and ID
                    Text = $"{s.Name} {s.Surname}",
                    Value = s.SurgeonId.ToString()
                })
                .ToList();

            // Pass an empty view model to the view with a default PrescriptionDate
            return View(new SurgeonPrescriptionViewModel());
        }

        // GET method to fetch dosage forms based on selected medication name
        [HttpGet]
        public JsonResult GetDosageForms(string medicationName)
        {
            // Retrieve distinct dosage forms for the selected medication
            var dosageForms = _context.Medication
                .Where(m => m.MedicationName == medicationName)
                .OrderBy(m => m.DosageForm) // Sort by DosageForm A-Z
                .Select(m => m.DosageForm)
                .Distinct()
                .ToList();

            // Return the dosage forms as JSON
            return Json(dosageForms);
        }

        // GET method to fetch patient details based on PatientIDNumber
        [HttpGet]
        public JsonResult GetPatientDetails(string patientIDNumber)
        {
            // Retrieve patient details based on the provided ID number
            var patient = _context.Patients
                .Where(p => p.PatientIDNumber == patientIDNumber)
                .Select(p => new
                {
                    // Return patient name and surname
                    p.PatientName,
                    p.PatientSurname
                })
                .FirstOrDefault();

            // Return the patient details as JSON
            return Json(patient);
        }

        [HttpPost]
        public IActionResult SurgeonPrescription(SurgeonPrescriptionViewModel model)
        {
            // Use the injected ApplicationDbContext
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Medications != null && model.Medications.Any())
                    {
                        // Determine the next PrescribedId
                        int nextPrescribedId = _context.SurgeonPrescription
                            .OrderByDescending(sp => sp.PrescribedID)
                            .Select(sp => sp.PrescribedID)
                            .FirstOrDefault() + 1;

                        // Get unique medications by MedicationName and DosageForm
                        var uniqueMedications = model.Medications
                            .GroupBy(m => new { m.MedicationName, m.PrescriptionDosageForm })
                            .Select(g => g.First())
                            .ToList();

                        foreach (var med in uniqueMedications)
                        {
                            var medicationItem = _context.Medication
                                .Where(m => m.MedicationName == med.MedicationName &&
                                            m.DosageForm == med.PrescriptionDosageForm &&
                                            m.IsDeleted != "Deleted")
                                .FirstOrDefault();

                            if (medicationItem != null)
                            {
                                var prescription = new SurgeonPrescription
                                {
                                    PatientIdnumber = model.PatientIDNumber,
                                    PatientName = model.PatientName,
                                    PatientSurname = model.PatientSurname,
                                    MedicationId = medicationItem.MedicationId,
                                    MedicationName = med.MedicationName,
                                    PrescriptionDosageForm = med.PrescriptionDosageForm,
                                    SurgeonId = model.SurgeonID,
                                    Quantity = med.Quantity,
                                    Instructions = med.Instructions,
                                    Urgent = model.Urgent,
                                    PrescriptionDate = model.PrescriptionDate,
                                    Dispense = model.Dispense,
                                    DispenseDateTime = model.Dispense == "Dispense" ? DateTime.Now : (DateTime?)null,
                                    PharmacistName = model.PharmacistName,
                                    PharmacistSurname = model.PharmacistSurname,
                                    PrescribedID = nextPrescribedId // Assign the same PrescribedId
                                };

                                // Add the new prescription to the database
                                _context.SurgeonPrescription.Add(prescription);
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, $"Medication {med.MedicationName} not found.");
                            }
                        }

                        // Save changes to the database
                        _context.SaveChanges();

                        return RedirectToAction("Success");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "At least one medication is required.");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ExceptionMessage = ex.Message;
                    return View(model);
                }
            }

            return View(model);
        }

    }
}