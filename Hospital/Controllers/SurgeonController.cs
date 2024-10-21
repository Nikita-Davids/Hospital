using Hospital.Data;
using Hospital.Models;
using Hospital.ModelViews;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

        // Displays a list of rejected prescriptions
        public IActionResult SurgeonAlerts()
        {
            // Retrieve the user's full name from TempData
            ViewBag.UserName = TempData["UserName"];

            // Group rejected prescriptions by PrescribedID and map fields from both RejectedPrescription and SurgeonPrescription
            var groupedRejectedPrescriptions = _context.RejectedPrescription
                .GroupBy(rp => rp.PrescribedID)
                .Select(g => new RejectedPrescriptionViewModel
                {
                    PrescribedID = g.Key,
                    PatientIDNumber = _context.SurgeonPrescription
                        .Where(p => p.PrescribedID == g.Key)
                        .Select(p => p.PatientIdnumber)
                        .FirstOrDefault(),
                    PatientName = _context.SurgeonPrescription
                        .Where(p => p.PrescribedID == g.Key)
                        .Select(p => p.PatientName)
                        .FirstOrDefault(),
                    PatientSurname = _context.SurgeonPrescription
                        .Where(p => p.PrescribedID == g.Key)
                        .Select(p => p.PatientSurname)
                        .FirstOrDefault(),
                    MedicationName = _context.SurgeonPrescription
                        .Where(p => p.PrescribedID == g.Key)
                        .Select(p => p.MedicationName)
                        .FirstOrDefault(),
                    PrescriptionDosageForm = _context.SurgeonPrescription
                        .Where(p => p.PrescribedID == g.Key)
                        .Select(p => p.PrescriptionDosageForm)
                        .FirstOrDefault(),
                    Quantity = _context.SurgeonPrescription
                        .Where(p => p.PrescribedID == g.Key)
                        .Select(p => p.Quantity)
                        .FirstOrDefault(),
                    Instructions = _context.SurgeonPrescription
                        .Where(p => p.PrescribedID == g.Key)
                        .Select(p => p.Instructions)
                        .FirstOrDefault(),
                    Urgent = _context.SurgeonPrescription
                        .Where(p => p.PrescribedID == g.Key)
                        .Select(p => p.Urgent)
                        .FirstOrDefault(),

                    // Map Rejection fields from RejectedPrescription
                    RejectionReason = g.FirstOrDefault().RejectionReason,
                    RejectionDate = g.FirstOrDefault().RejectionDate,
                    PharmacistName = g.FirstOrDefault().PharmacistName,
                    PharmacistSurname = g.FirstOrDefault().PharmacistSurname,
                    Status = g.FirstOrDefault().Status
                })
                .OrderByDescending(p => p.RejectionDate)
                .ToList();

            return View(groupedRejectedPrescriptions);
        }

        // Update the status from "Pending" to "Seen" and redirect to the details view
        public IActionResult ViewRejectedPrescription(int id)
        {
            // Find the rejected prescription by ID
            var rejectedPrescription = _context.RejectedPrescription
                .FirstOrDefault(rp => rp.PrescribedID == id);

            if (rejectedPrescription == null)
            {
                return NotFound();
            }

            // Update the status to "Seen"
            rejectedPrescription.Status = "Seen";
            _context.SaveChanges();

            // Redirect to the details view
            return RedirectToAction("RejectedPrescriptionDetails", new { id });
        }

        // Display detailed view of the rejected prescription
        public IActionResult RejectedPrescriptionDetails(int id)
        {
            // Retrieve the rejected prescription details by ID
            var rejectedPrescription = _context.RejectedPrescription
                .FirstOrDefault(rp => rp.PrescribedID == id);

            if (rejectedPrescription == null)
            {
                return NotFound();
            }

            // Pass the prescription details to the view
            return View(rejectedPrescription);
        }
    

    public async Task<IActionResult> SurgeonViewBookedPatient()
        {
            var bookedpatient = await _context.BookingSurgery.ToListAsync();
            return View(bookedpatient);
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
            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure that there are medications to process
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
                                // Add a model error if the medication is not found
                                ModelState.AddModelError(string.Empty, $"Medication {med.MedicationName} not found.");
                            }
                        }

                        // Save changes to the database
                        _context.SaveChanges();

                        // Redirect to the success page upon successful submission
                        return RedirectToAction("Success");
                    }
                    else
                    {
                        // Add a model error if no medications are provided
                        ModelState.AddModelError(string.Empty, "At least one medication is required.");
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions and pass the error message to the view
                    ViewBag.ExceptionMessage = ex.Message;
                    return View(model);
                }
            }

            // If model state is invalid, redisplay the form with validation errors
            return View(model);
        }

        public async Task<IActionResult> SurgeonPatientOverview(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest("Patient ID is required.");
            }

            // Fetch patient details
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientIDNumber == patientId);
            if (patient == null)
            {
                return NotFound();
            }

            // Fetch vitals (allow null if not found)
            var vitals = await _context.PatientVital.FirstOrDefaultAsync(v => v.PatientId == patientId);

            // Fetch allergies (empty list if not found)
            var allergies = await _context.PatientAllergies.Where(a => a.PatientId == patientId).ToListAsync();

            // Fetch current medications (empty list if not found)
            var currentMedications = await _context.PatientCurrentMedication.Where(m => m.PatientId == patientId).ToListAsync();

            // Fetch medical conditions (empty list if not found)
            var medicalConditions = await _context.PatientMedicalCondition.Where(c => c.PatientId == patientId).ToListAsync();

            // Construct the model with vitals and other details (using null-coalescing operator where necessary)
            var model = new PatientOverviewViewModel
            {
                PatientIDNumber = patient.PatientIDNumber,
                PatientName = patient.PatientName,
                PatientSurname = patient.PatientSurname,
                PatientAddress = patient.PatientAddress,
                PatientContactNumber = patient.PatientContactNumber,
                PatientEmailAddress = patient.PatientEmailAddress,
                PatientDateOfBirth = patient.PatientDateOfBirth,
                PatientGender = patient.PatientGender,
                Weight = vitals?.Weight ?? null, // Allow null if vitals are not found
                Height = vitals?.Height ?? null,
                Temperature = vitals?.Tempreture ?? null,
                BloodPressure = vitals?.BloodPressure ?? null,
                Pulse = vitals?.Pulse ?? null,
                Respiratory = vitals?.Respiratory ?? null,
                BloodOxygen = vitals?.BloodOxygen ?? null,
                BloodGlucoseLevel = vitals?.BloodGlucoseLevel ?? null,
                VitalTime = vitals?.VitalTime ?? null,
                Allergies = allergies, // Will be an empty list if none are found
                CurrentMedications = currentMedications, // Empty list if none found
                MedicalConditions = medicalConditions // Empty list if none found
            };

            return View(model);
        }

        public IActionResult SurgeonDischargePatients()
        {
            // Fetch the list of patients and create a concatenated display name
            var patients = _context.Patients
                .Select(p => new
                {
                    PatientIDNumber = p.PatientIDNumber,
                    FullName = p.PatientName + " " + p.PatientSurname
                })
                .ToList();

            ViewBag.PatientId = new SelectList(patients, "PatientIDNumber", "FullName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonDischargePatients(DischargedPatient model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Patient Discharged successfully.";
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("SurgeonDischargePatients", "Surgeon");
            }
            return View(model);
        }

        // GET: Surgeon/EditDischargedPatient/5
        public async Task<IActionResult> SurgeonEditDischargedPatient(int id)
        {
            // Fetch the discharged patient record from the database
            var dischargedPatient = await _context.DischargedPatients.FindAsync(id);
            if (dischargedPatient == null)
            {
                return NotFound();
            }

            // Populate ViewBag for dropdown list, if needed (e.g., for selecting patients)
            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientIDNumber", dischargedPatient.PatientId);

            return View(dischargedPatient);
        }

        // POST: Surgeon/EditDischargedPatient/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonEditDischargedPatient(int id, DischargedPatient model)
        {
            if (id != model.DischargedPatients)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the discharged patient record
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DischargedPatientExists(model.DischargedPatients))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("SurgeonViewDischargedPatients");
            }

            // Repopulate ViewBag for dropdown list in case of a validation failure
            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientIDNumber", model.PatientId);

            return View(model);
        }

        // Check if a discharged patient record exists
        private bool DischargedPatientExists(int id)
        {
            return _context.DischargedPatients.Any(e => e.DischargedPatients == id);
        }


        public IActionResult SurgeonViewDischargedPatients()
        {
            // Fetch all DischargedPatient entries
            var dischargedPatients = _context.DischargedPatients.ToList();
            return View(dischargedPatients);
        }

        // GET: Surgeon/AddPatient
        public IActionResult SurgeonAddPatients()
        {
            return View();
        }

        // POST: Surgeon/AddPatient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonAddPatients(Patients model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Patient added successfully.";
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("SurgeonAddPatients");
            }
            return View(model);
        }

        // GET: Surgeon/ViewPatients
        public async Task<IActionResult> SurgeonViewPatients()
        {
            var patients = await _context.Patients.ToListAsync();
            return View(patients);
        }
        // GET: Surgeon/EditPatient/5
        public async Task<IActionResult> SurgeonEditPatients(string id)
        {
            // Fetch the patient record from the database
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }
        // POST: Surgeon/EditPatient/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonEditPatients(string id, Patients model)
        {
            if (id != model.PatientIDNumber)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(model.PatientIDNumber))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("SurgeonViewPatients");
            }
            return View(model);
        }

        private bool PatientExists(string id)
        {
            return _context.Patients.Any(e => e.PatientIDNumber == id);
        }




        // GET: Surgeon/AddPatientVital
        public IActionResult SurgeonAddPatientVital()
        {
            // Populate ViewBag.PatientId with a list of patients for the dropdown
            var patients = _context.Patients
               .Select(p => new
               {
                   PatientIDNumber = p.PatientIDNumber,
                   FullName = p.PatientName + " " + p.PatientSurname
               })
               .ToList();

            ViewBag.PatientId = new SelectList(patients, "PatientIDNumber", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonAddPatientVital(PatientVital model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    TempData["SuccessMessage"] = "Patient Vitals added successfully.";
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("SurgeonAddPatientVital");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine(ex.Message);
                    ModelState.AddModelError("", "An error occurred while saving the data.");
                }
            }

            // Repopulate dropdown list in case of validation failure
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName", model.PatientId);
            return View(model);
        }

        // GET: Surgeon/ViewPatients
        public async Task<IActionResult> SurgeonViewPatientVital()
        {
            var patientvitals = await _context.PatientVital.ToListAsync();
            return View(patientvitals);
        }

        public async Task<IActionResult> SurgeonEditPatientVital(int id)
        {
            // Fetch the patient vital record from the database
            var patientVital = await _context.PatientVital.FindAsync(id);
            if (patientVital == null)
            {
                return NotFound();
            }

            // Populate ViewBag for dropdown list, if applicable
            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientIDNumber", patientVital.PatientId);

            return View(patientVital);
        }
        // POST: Surgeon/EditPatientVital/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonEditPatientVital(int id, PatientVital model)
        {
            if (id != model.PatientVitalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientVitalExists(model.PatientVitalId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("SurgeonViewPatientVital");
            }

            // Populate ViewBag for dropdown list, if applicable
            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientIDNumber", model.PatientId);

            return View(model);
        }
        private bool PatientVitalExists(int id)
        {
            return _context.PatientVital.Any(e => e.PatientVitalId == id);
        }

        // GET: Surgeon/AddPatientAllergy
        [HttpGet]
        public IActionResult SurgeonAddPatientAllergy()
        {
            // Populate ViewBag.PatientId with a list of patients for the dropdown
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName");
            ViewBag.ActiveId = new SelectList(_context.ActiveIngredient, "IngredientId", "IngredientName");
            return View(new PatientAllergy());
        }

        // POST: Surgeon/AddPatientAllergy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SurgeonAddPatientAllergy(PatientAllergy model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Patient Allergy added successfully.";
                // Check if an allergy entry with the same patient and allergy already exists
                var existingAllergy = _context.PatientAllergies
                    .FirstOrDefault(pa => pa.PatientId.Trim().ToLower() == model.PatientId.Trim().ToLower() &&
                                          pa.Allergy.Trim().ToLower() == model.Allergy.Trim().ToLower());

                if (existingAllergy == null)
                {
                    // Create a new PatientAllergy entity
                    var patientAllergy = new PatientAllergy
                    {
                        PatientId = model.PatientId,
                        Allergy = model.Allergy
                    };

                    // Add the PatientAllergy entity to the context
                    _context.PatientAllergies.Add(patientAllergy);
                    _context.SaveChanges(); // Save changes

                    // Redirect to the success page after successful addition
                    return RedirectToAction("SurgeonAddPatientAllergy"); // Change this to your actual action method
                }
                else
                {
                    // If the allergy entry with the same patient and allergy already exists, return an error message
                    ModelState.AddModelError("", "this allergy entry for this patient already exists.");
                }
            }

            // Repopulate dropdown list in case of validation failure
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName", model.PatientId);
            ViewBag.ActiveId = new SelectList(_context.ActiveIngredient, "IngredientId", "IngredientName", model.PatientId);
            return View(model);
        }


        // GET: 
        public async Task<IActionResult> SurgeonViewPatientAllergy()
        {
            var patientalllergy = await _context.PatientAllergies.ToListAsync();
            return View(patientalllergy);
        }
        // GET: PatientAllergy/SurgeonEditPatientAllergy/5
        public async Task<IActionResult> SurgeonEditPatientAllergy(int id)
        {
            // Retrieve the patient allergy record from the database using the provided ID
            var patientAllergy = await _context.PatientAllergies
                .FirstOrDefaultAsync(m => m.AllergyId == id);

            // Check if the patient allergy exists. If not, return a 404 Not Found response.
            if (patientAllergy == null)
            {
                return NotFound();
            }

            // Populate ViewBag with a list of patients for the dropdown
            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientId", "PatientName", patientAllergy.PatientId);

            // Return the view with the patient allergy data to be edited
            return View(patientAllergy);
        }

        // POST: PatientAllergy/SurgeonEditPatientAllergy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonEditPatientAllergy(int id, [Bind("AllergyId,PatientId,Allergy")] PatientAllergy patientAllergy)
        {
            // Check if the ID in the URL matches the ID of the patient allergy being edited
            if (id != patientAllergy.AllergyId)
            {
                return NotFound();
            }

            // Check if the allergy for the same patient already exists (excluding the current entry)
            if (_context.PatientAllergies.Any(pa => pa.PatientId == patientAllergy.PatientId && pa.Allergy == patientAllergy.Allergy && pa.AllergyId != id))
            {
                // Add a model error if the allergy already exists and return the view with the error
                ModelState.AddModelError("Allergy", "This allergy already exists for the selected patient.");
                // Populate ViewBag with a list of patients for the dropdown if validation fails
                ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientId", "PatientName", patientAllergy.PatientId);
                return View(patientAllergy);
            }

            // If the model state is valid, update the patient allergy in the database
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patientAllergy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientAllergyExists(patientAllergy.AllergyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the index or view action if the update was successful
                return RedirectToAction(nameof(SurgeonViewPatientAllergy));
            }

            // Populate ViewBag with a list of patients for the dropdown if the model state is not valid
            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientId", "PatientName", patientAllergy.PatientId);
            return View(patientAllergy);
        }

        // Helper method to check if a patient allergy exists in the database
        private bool PatientAllergyExists(int id)
        {
            return _context.PatientAllergies.Any(e => e.AllergyId == id);
        }





        // GET: Surgeon/AddPatientCurrentMedication
        [HttpGet]
        public IActionResult SurgeonAddCurrentMedication()
        {
            // Populate ViewBag.PatientId with a list of patients for the dropdown
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName");
            return View(new PatientCurrentMedication());
        }

        // POST: Surgeon/AddPatientCurrentMedication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonAddCurrentMedication(PatientCurrentMedication model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Patient Medication added successfully.";
                // Check if a current medication entry with the same patient and medication already exists
                var existingMedication = await _context.PatientCurrentMedication
                    .FirstOrDefaultAsync(pm => pm.PatientId.Trim().ToLower() == model.PatientId.Trim().ToLower() &&
                                               pm.CurrentMedication.Trim().ToLower() == model.CurrentMedication.Trim().ToLower());

                if (existingMedication == null)
                {
                    // Create a new PatientCurrentMedication entity
                    var patientMedication = new PatientCurrentMedication
                    {
                        PatientId = model.PatientId,
                        CurrentMedication = model.CurrentMedication
                    };

                    // Add the PatientCurrentMedication entity to the context
                    _context.PatientCurrentMedication.Add(patientMedication);
                    await _context.SaveChangesAsync(); // Save changes

                    // Redirect to the success page after successful addition
                    return RedirectToAction("SurgeonAddCurrentMedication");
                }
                else
                {
                    // If the medication entry with the same patient and medication already exists, return an error message
                    ModelState.AddModelError("", "A medication entry for this patient already exists.");
                }
            }

            // Repopulate dropdown list in case of validation failure
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName", model.PatientId);
            return View(model);
        }

        // GET: Surgeon/ViewPatientCurrentMedication
        public async Task<IActionResult> SurgeonViewPatientCurrentMedication()
        {
            var patientMedications = await _context.PatientCurrentMedication.ToListAsync();
            return View(patientMedications);
        }

        // GET: Surgeon/EditPatientCurrentMedication/5
        public async Task<IActionResult> SurgeonEditCurrentMedication(int id)
        {
            // Retrieve the patient current medication record from the database using the provided ID
            var patientMedication = await _context.PatientCurrentMedication
                .FirstOrDefaultAsync(pm => pm.MedicationId == id);


            // Check if the patient medication exists. If not, return a 404 Not Found response.
            if (patientMedication == null)
            {
                return NotFound();
            }

            // Populate ViewBag with a list of patients for the dropdown
            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientName", patientMedication.PatientId);

            // Return the view with the patient medication data to be edited
            return View(patientMedication);
        }

        // POST: Surgeon/EditPatientCurrentMedication/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonEditCurrentMedication(int id, [Bind("MedicationId,PatientId,CurrentMedication")] PatientCurrentMedication patientMedication)
        {
            // Check if the ID in the URL matches the ID of the patient medication being edited
            if (id != patientMedication.MedicationId)
            {
                return NotFound();
            }

            // Check if the medication for the same patient already exists (excluding the current entry)
            if (await _context.PatientCurrentMedication
                .AnyAsync(pm => pm.PatientId == patientMedication.PatientId &&
                                pm.CurrentMedication == patientMedication.CurrentMedication &&
                                pm.MedicationId != id))
            {
                // Add a model error if the medication already exists and return the view with the error
                ModelState.AddModelError("CurrentMedication", "This medication already exists for the selected patient.");
                // Populate ViewBag with a list of patients for the dropdown if validation fails
                ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientName", patientMedication.PatientId);
                return View(patientMedication);
            }

            // If the model state is valid, update the patient medication in the database
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patientMedication);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientCurrentMedicationExists(patientMedication.MedicationId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the index or view action if the update was successful
                return RedirectToAction(nameof(SurgeonViewPatientCurrentMedication));
            }

            // Populate ViewBag with a list of patients for the dropdown if the model state is not valid
            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientName", patientMedication.PatientId);
            return View(patientMedication);
        }

        // Helper method to check if a patient current medication exists in the database
        private bool PatientCurrentMedicationExists(int id)
        {
            return _context.PatientCurrentMedication.Any(e => e.MedicationId == id);
        }





        // GET: Surgeon/AddPatientMedicalCondition
        [HttpGet]
        public IActionResult SurgeonAddMedicalCondition()
        {
            // Populate ViewBag.PatientId with a list of patients for the dropdown
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName");
            return View(new PatientMedicalCondition());
        }
        // POST: Surgeon/AddPatientMedicalCondition
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonAddMedicalCondition(PatientMedicalCondition model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Patient Medical Condition added successfully.";
                // Check if a medical condition entry with the same patient and condition already exists
                var existingCondition = await _context.PatientMedicalCondition
                    .FirstOrDefaultAsync(pc => pc.PatientId.Trim().ToLower() == model.PatientId.Trim().ToLower() &&
                                               pc.MedicalCondition.Trim().ToLower() == model.MedicalCondition.Trim().ToLower());

                if (existingCondition == null)
                {
                    // Create a new PatientMedicalCondition entity
                    var patientCondition = new PatientMedicalCondition
                    {
                        PatientId = model.PatientId,
                        MedicalCondition = model.MedicalCondition
                    };

                    // Add the PatientMedicalCondition entity to the context
                    _context.PatientMedicalCondition.Add(patientCondition);
                    await _context.SaveChangesAsync(); // Save changes

                    // Redirect to the success page after successful addition
                    return RedirectToAction("SurgeonAddMedicalCondition");
                }
                else
                {
                    // If the condition entry with the same patient and condition already exists, return an error message
                    ModelState.AddModelError("", "A medical condition entry for this patient already exists.");
                }
            }

            // Repopulate dropdown list in case of validation failure
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName", model.PatientId);
            return View(model);
        }
        // GET: 
        public async Task<IActionResult> SurgeonViewPatientMedicalCondition()
        {
            var patientcurrentmedicalcondition = await _context.PatientMedicalCondition.ToListAsync();
            return View(patientcurrentmedicalcondition);
        }
        // GET: Surgeon/EditPatientMedicalCondition/5
        public async Task<IActionResult> SurgeonEditMedicalCondition(int id)
        {
            // Retrieve the patient medical condition record from the database using the provided ID
            var patientCondition = await _context.PatientMedicalCondition
                .FirstOrDefaultAsync(pc => pc.ConditionId == id);

            // Check if the patient condition exists. If not, return a 404 Not Found response.
            if (patientCondition == null)
            {
                return NotFound();
            }

            // Populate ViewBag with a list of patients for the dropdown
            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientName", patientCondition.PatientId);

            // Return the view with the patient condition data to be edited
            return View(patientCondition);
        }
        // POST: Surgeon/EditPatientMedicalCondition/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonEditMedicalCondition(int id, [Bind("ConditionId,PatientId,MedicalCondition")] PatientMedicalCondition patientCondition)
        {
            // Check if the ID in the URL matches the ID of the patient condition being edited
            if (id != patientCondition.ConditionId)
            {
                return NotFound();
            }

            // Check if the condition for the same patient already exists (excluding the current entry)
            if (await _context.PatientMedicalCondition
                .AnyAsync(pc => pc.PatientId == patientCondition.PatientId &&
                                pc.MedicalCondition == patientCondition.MedicalCondition &&
                                pc.ConditionId != id))
            {
                // Add a model error if the condition already exists and return the view with the error
                ModelState.AddModelError("MedicalCondition", "This condition already exists for the selected patient.");
                // Populate ViewBag with a list of patients for the dropdown if validation fails
                ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientName", patientCondition.PatientId);
                return View(patientCondition);
            }

            // If the model state is valid, update the patient condition in the database
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patientCondition);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientMedicalConditionExists(patientCondition.ConditionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the index or view action if the update was successful
                return RedirectToAction(nameof(SurgeonViewPatientMedicalCondition));
            }

            // Populate ViewBag with a list of patients for the dropdown if the model state is not valid
            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientName", patientCondition.PatientId);
            return View(patientCondition);
        }
        // Helper method to check if a patient medical condition exists in the database
        private bool PatientMedicalConditionExists(int id)
        {
            return _context.PatientMedicalCondition.Any(e => e.ConditionId == id);
        }


        // GET: Surgeon/SurgeonAddAdministerMedication
        public IActionResult SurgeonAddAdministerMedication(string patientId, string patientName, string medicationName, int medicationId, string dosageForm, int quantity)
        {
            // If patient data is provided, populate the form fields using ViewBag
            if (!string.IsNullOrEmpty(patientId))
            {
                ViewBag.PatientId = patientId;
                ViewBag.PatientName = patientName;
                ViewBag.ScriptDetails = medicationName; // Populate ScriptDetails with the Medication Name
                ViewBag.MedicationId = medicationId;
                ViewBag.DosageForm = dosageForm;
                ViewBag.Quantity = quantity;
            }

            // Optionally, you could fetch additional data here from the database if needed
            // For example: Fetch a full list of medications for dropdown
            // ViewBag.Medications = new SelectList(_context.Medications, "MedicationId", "MedicationName");

            return View();
        }

        // POST: Surgeon/SurgeonAddAdministerMedication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonAddAdministerMedication(AdministerMedication model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save the new medication administration record to the database
                    _context.Add(model);
                    await _context.SaveChangesAsync();

                    // Display a success message
                    TempData["SuccessMessage"] = "Medication administered successfully.";

                    return RedirectToAction("SurgeonAddAdministerMedication"); // Redirect to the list of administered medications
                }
                catch (Exception ex)
                {
                    // Log the error
                    Console.WriteLine(ex.Message);
                    ModelState.AddModelError("", "An error occurred while saving the data.");
                }
            }

            // If there is a validation error or an exception, repopulate the form with the submitted values
            ViewBag.PatientId = model.Patient_Id;
            ViewBag.ScriptDetails = model.ScriptDetails; // Populate with the medication name
            ViewBag.MedicationId = model.MedicationId;
            ViewBag.DosageForm = model.DosageFormName;
            ViewBag.Quantity = model.Quantity;

            return View(model); // Return the form view with validation messages
        }

        public IActionResult SurgeonViewAdministerMedication()
        {
            // Fetch all DischargedPatient entries
            var administerMedication = _context.AdministerMedication.ToList();
            return View(administerMedication);
        }

        public async Task<IActionResult> SurgeonEditAdministerMedication(int id)
        {
            // Fetch the administer medication record from the database
            var administerMedication = await _context.AdministerMedication.FindAsync(id);
            if (administerMedication == null)
            {
                return NotFound();
            }

            // Populate ViewBag for dropdown list, if applicable
            // Assuming you want to show a dropdown for MedicationId, if applicable
            ViewBag.MedicationId = new SelectList(await _context.Medication.ToListAsync(), "MedicationId", "MedicationName", administerMedication.MedicationId);

            return View(administerMedication);
        }

        // POST: Surgeon/EditAdministerMedication/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonEditAdministerMedication(int id, AdministerMedication model)
        {
            if (id != model.AdministerMedication_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdministerMedicationExists(model.AdministerMedication_Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("SurgeonViewAdministerMedication");
            }

            // Populate ViewBag for dropdown list again, if applicable
            ViewBag.MedicationId = new SelectList(await _context.Medication.ToListAsync(), "MedicationId", "MedicationName", model.MedicationId);

            return View(model);
        }

        private bool AdministerMedicationExists(int id)
        {
            return _context.AdministerMedication.Any(e => e.AdministerMedication_Id == id);
        }


        public async Task<IActionResult> SurgeonBookingSurgery()
        {
            // Populate ViewBag.PatientId with a list of patients for the dropdown
            var patients = _context.Patients
               .Select(p => new
               {
                   PatientIDNumber = p.PatientIDNumber,
                   FullName = p.PatientName + " " + p.PatientSurname
               })
               .ToList();

            ViewBag.PatientId = new SelectList(patients, "PatientIDNumber", "FullName");

            // Populate ViewBag.TreatmentCodeId with a list of chronic medications for the dropdown
            var treatmentCodes = _context.TreatmentCode
                .Select(tc => new
                {
                    tc.TreatmentCodeId, // Use the primary key as value
                    Description = tc.TreatmentCodeDescription + " - " + tc.Icd10Code
                })
                .ToList();

            ViewBag.TreatmentCodeId = new SelectList(treatmentCodes, "Description", "Description");

            // Fetch the list of operating theatres
            var operatingTheatres = _context.OperatingTheatre
                .Select(ot => new
                {
                    ot.OperatingTheatreId,
                    ot.OperatingTheatreName
                })
                .ToList();
            ViewBag.OperatingTheatreId = new SelectList(operatingTheatres, "OperatingTheatreName", "OperatingTheatreName");


            return View(new BookingSurgery());
        }
        [HttpGet]
        public async Task<IActionResult> GetPatientEmail(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return Json(new { email = "" });
            }

            var patient = await _context.Patients
                .Where(p => p.PatientIDNumber == patientId)
                .Select(p => new
                {
                    Email = p.PatientEmailAddress // Adjust this to your actual email property name
                })
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return Json(new { email = "" }); // Return empty if no patient found
            }

            return Json(new { email = patient.Email }); // Return the email as JSON
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SurgeonBookingSurgery(BookingSurgery bookingSurgery)
        {
            if (ModelState.IsValid)
            {
                // Add the booking surgery to the database
                _context.Add(bookingSurgery);
                await _context.SaveChangesAsync();

                // Display success message
                TempData["SuccessMessage"] = "Surgery successfully booked!";

                // Redirect to some page (e.g., back to the list)
                return RedirectToAction("SurgeonBookingSurgery","Surgeon");
            }

            // If model validation fails, reload the dropdown data and redisplay the form
            var patients = _context.Patients
                .Select(p => new
                {
                    PatientIDNumber = p.PatientIDNumber,
                    FullName = p.PatientName + " " + p.PatientSurname
                })
                .ToList();
            ViewBag.PatientId = new SelectList(patients, "PatientIDNumber", "FullName");

            var treatmentCodes = _context.TreatmentCode
                .Select(tc => new
                {
                    tc.TreatmentCodeId,
                    Description = tc.TreatmentCodeDescription + " - " + tc.Icd10Code
                })
                .ToList();

            ViewBag.TreatmentCodeId = new SelectList(treatmentCodes, "TreatmentCodeId", "Description");

            // Fetch the list of operating theatres
            var operatingTheatres = _context.OperatingTheatre
                .Select(ot => new
                {
                    ot.OperatingTheatreId,
                    ot.OperatingTheatreName
                })
                .ToList();
            ViewBag.OperatingTheatreId = new SelectList(operatingTheatres, "OperatingTheatreName", "OperatingTheatreName");


            // Return the form with validation errors
            return View(bookingSurgery);
        }

        // GET: Nurse/ViewPatientCurrentMedication
        public async Task<IActionResult> NurseViewPatientCurrentMedication()
        {
            var patientMedications = await _context.PatientCurrentMedication.ToListAsync();
            return View(patientMedications);
        }
        // GET: Nurse/ViewPatientCurrentMedication
        public async Task<IActionResult> SurgeonViewBookingSurgery()
        {
            var patientMedications = await _context.BookingSurgery.ToListAsync();
            return View(patientMedications);
        }
    }
}
