using Hospital.Data;
using Hospital.Models;
using Hospital.ModelViews;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace Hospital.Controllers
{
    public class NurseController(ApplicationDbContext dbContext) : Controller
    {
        ApplicationDbContext _context = dbContext;

        public IActionResult NurseIndex()
        {
            // Retrieve the user's full name from TempData
            ViewBag.UserName = TempData["UserName"];

            // Retrieve all patient vital records
            var patientVitals = _context.PatientVital
                .Select(v => new PatientVital
                {
                    PatientVitalId = v.PatientVitalId,
                    PatientId = v.PatientId,
                    Weight = v.Weight,
                    Height = v.Height,
                    Tempreture = v.Tempreture,
                    BloodPressure = v.BloodPressure,
                    Pulse = v.Pulse,
                    Respiratory = v.Respiratory,
                    BloodOxygen = v.BloodOxygen,
                    BloodGlucoseLevel = v.BloodGlucoseLevel,
                    VitalTime = v.VitalTime
                })
                .OrderByDescending(v => v.VitalTime)
                .ToList();

            // Pass the list of patient vitals to the view
            return View(patientVitals);
        }

        public IActionResult NurseDischargePatients()
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
        public async Task<IActionResult> NurseDischargePatients(DischargedPatient model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Patient Discharged successfully.";
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("NurseDischargePatients", "Nurse");
            }
            return View(model);
        }
        public IActionResult NurseViewDischarges()
        {
            // Fetch all DischargedPatient entries
            var dischargedPatients = _context.DischargedPatients.ToList();
            return View(dischargedPatients);
        }
       



        // GET: Nurse/AddPatient
        public IActionResult NurseAddPatients()
        {
            return View();
        }

        // POST: Nurse/AddPatient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddPatients(Patients model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Patient added successfully.";
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("NurseAddPatients");
            }
            return View(model);
        }

        // GET: Nurse/ViewPatients
        public async Task<IActionResult> NurseViewPatients()
        {
            var patients = await _context.Patients.ToListAsync();
            return View(patients);
        }
        // GET: Nurse/EditPatient/5
        public async Task<IActionResult> NurseEditPatients(string id)
        {
            // Fetch the patient record from the database
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }
        // POST: Nurse/EditPatient/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseEditPatients(string id, Patients model)
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
                return RedirectToAction("NurseViewPatients");
            }
            return View(model);
        }

        private bool PatientExists(string id)
        {
            return _context.Patients.Any(e => e.PatientIDNumber == id);
        }




        // GET: Nurse/AddPatientVital
        public IActionResult NurseAddPatientVital()
        {
            // Populate ViewBag.PatientId with a list of patients for the dropdown
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddPatientVital(PatientVital model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    TempData["SuccessMessage"] = "Patient Vitals added successfully.";
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("NurseAddPatientVital");
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

        // GET: Nurse/ViewPatients
        public async Task<IActionResult> NurseViewPatientVital()
        {
            var patientvitals = await _context.PatientVital.ToListAsync();
            return View(patientvitals);
        }
       
        public async Task<IActionResult> NurseEditPatientVital(int id)
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
        // POST: Nurse/EditPatientVital/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseEditPatientVital(int id, PatientVital model)
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
                return RedirectToAction("NurseViewPatientVital");
            }

            // Populate ViewBag for dropdown list, if applicable
            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientIDNumber", model.PatientId);

            return View(model);
        }
        private bool PatientVitalExists(int id)
        {
            return _context.PatientVital.Any(e => e.PatientVitalId == id);
        }






        // GET: Nurse/AddPatientAllergy
        [HttpGet]
        public IActionResult NurseAddPatientAllergy()
        {
            // Populate ViewBag.PatientId with a list of patients for the dropdown
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName");
            ViewBag.ActiveId = new SelectList(_context.ActiveIngredient, "IngredientId", "IngredientName");
            return View(new PatientAllergy());
        }

        // POST: Nurse/AddPatientAllergy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NurseAddPatientAllergy(PatientAllergy model)
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
                    return RedirectToAction("NurseAddPatientAllergy"); // Change this to your actual action method
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
        public async Task<IActionResult> NurseViewPatientAllergy()
        {
            var patientalllergy = await _context.PatientAllergies.ToListAsync();
            return View(patientalllergy);
        }
        // GET: PatientAllergy/NurseEditPatientAllergy/5
        public async Task<IActionResult> NurseEditPatientAllergy(int id)
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

        // POST: PatientAllergy/NurseEditPatientAllergy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseEditPatientAllergy(int id, [Bind("AllergyId,PatientId,Allergy")] PatientAllergy patientAllergy)
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
                return RedirectToAction(nameof(NurseViewPatientAllergy));
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





        // GET: Nurse/AddPatientCurrentMedication
        [HttpGet]
        public IActionResult NurseAddCurrentMedication()
        {
            // Populate ViewBag.PatientId with a list of patients for the dropdown
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName");
            return View(new PatientCurrentMedication());
        }

        // POST: Nurse/AddPatientCurrentMedication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddCurrentMedication(PatientCurrentMedication model)
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
                    return RedirectToAction("NurseAddCurrentMedication");
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

        // GET: Nurse/ViewPatientCurrentMedication
        public async Task<IActionResult> NurseViewPatientCurrentMedication()
        {
            var patientMedications = await _context.PatientCurrentMedication.ToListAsync();
            return View(patientMedications);
        }

        // GET: Nurse/EditPatientCurrentMedication/5
        public async Task<IActionResult> NurseEditCurrentMedication(int id)
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

        // POST: Nurse/EditPatientCurrentMedication/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseEditCurrentMedication(int id, [Bind("MedicationId,PatientId,CurrentMedication")] PatientCurrentMedication patientMedication)
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
                return RedirectToAction(nameof(NurseViewPatientCurrentMedication));
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





        // GET: Nurse/AddPatientMedicalCondition
        [HttpGet]
        public IActionResult NurseAddMedicalCondition()
        {
            // Populate ViewBag.PatientId with a list of patients for the dropdown
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName");
            return View(new PatientMedicalCondition());
        }
        // POST: Nurse/AddPatientMedicalCondition
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddMedicalCondition(PatientMedicalCondition model)
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
                    return RedirectToAction("NurseAddMedicalCondition");
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
        public async Task<IActionResult> NurseViewPatientMedicalCondition()
        {
            var patientcurrentmedicalcondition = await _context.PatientMedicalCondition.ToListAsync();
            return View(patientcurrentmedicalcondition);
        }
        // GET: Nurse/EditPatientMedicalCondition/5
        public async Task<IActionResult> NurseEditMedicalCondition(int id)
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
        // POST: Nurse/EditPatientMedicalCondition/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseEditMedicalCondition(int id, [Bind("ConditionId,PatientId,MedicalCondition")] PatientMedicalCondition patientCondition)
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
                return RedirectToAction(nameof(NurseViewPatientMedicalCondition));
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

     










        public async Task SendPatientVitalsEmail(List<PatientVital> recordedVitals)
        {
            try
            {
                // Build the vitals details for the email body
                string vitalsDetails = "<ul>";

                foreach (var item in recordedVitals)
                {
                    // Retrieve the patient's information based on the PatientId
                    var patient = await _context.Patients
                        .Where(p => p.PatientIDNumber == item.PatientId)
                        .Select(p => new { p.PatientIDNumber, p.PatientName, p.PatientSurname })
                        .FirstOrDefaultAsync();

                    // Add patient's name and recorded vitals to the email body
                    if (patient != null)
                    {
                        vitalsDetails += $"<li>Patient: {patient.PatientName} {patient.PatientSurname} (ID: {patient.PatientIDNumber}), " +
                                         $"Weight: {item.Weight} kg, " +
                                         $"Height: {item.Height} cm, " +
                                         $"Temperature: {item.Tempreture}°C, " +
                                         $"Blood Pressure: {item.BloodPressure} mmHg, " +
                                         $"Pulse: {item.Pulse} bpm, " +
                                         $"Respiratory Rate: {item.Respiratory} breaths/min, " +
                                         $"Blood Oxygen: {item.BloodOxygen}%, " +
                                         $"Blood Glucose Level: {item.BloodGlucoseLevel} mg/dL, " +
                                         $"Time Recorded: {item.VitalTime?.ToString(@"hh\:mm")}</li>";
                    }
                }

                vitalsDetails += "</ul>";

                // Define email details
                var emailMessage = new MimeMessage
                {
                    From = { new MailboxAddress("Hospital Vitals Monitoring", "noreply@hospital.com") },
                    To = { new MailboxAddress("Surgeon", "gabrielkojo77@gmail.com") }, // Recipients email
                    Subject = "Patient Vitals Recorded",
                    Body = new BodyBuilder
                    {
                        HtmlBody = $@"
                        <h3>Patient Vitals Recorded</h3>
                        <p>The following vitals have been recorded in the system:</p>
                        {vitalsDetails}
                        <p>Thank you for using the hospital's patient monitoring system.</p>"
                    }.ToMessageBody()
                };

                // Send the email
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    // Connect to the SMTP server
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync("kitadavids1@gmail.com", "nhjj efnx mjpv okee");
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }

                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                // Log email sending errors
                Console.WriteLine($"Error occurred while sending email: {ex.Message}");
            }
        }
        public IActionResult NurseDispensedAlert()
        {

            ViewBag.UserName = TempData["UserName"];

            // Retrieve all surgeon prescriptions where Dispense is set to 'Dispense'
            var dispensedMedications = _context.SurgeonPrescription
                .Where(sp => sp.Dispense == "Dispense")
                .ToList();

            // Pass the list of dispensed medications to the view for display
            return View(dispensedMedications);
        }

        public IActionResult NurseDispensedDetails(string patientId)
        {
            // Retrieve the prescription details for the specific patient
            var prescriptionDetails = _context.SurgeonPrescription
                .Where(sp => sp.PatientIdnumber == patientId && sp.Dispense == "Dispense")
                .Select(sp => new
                {
                    sp.PrescriptionId,
                    sp.PatientIdnumber,
                    sp.PatientName,
                    sp.PatientSurname,
                    sp.MedicationName,
                    sp.PrescriptionDosageForm,
                    sp.Quantity
                })
                .FirstOrDefault();

            if (prescriptionDetails == null)
            {
                return NotFound();
            }

            return View(prescriptionDetails);
        }

        [HttpPost]
        public IActionResult ReceiveMedication(int prescriptionId)
        {
            // Find the prescription by the ID
            var prescription = _context.SurgeonPrescription.FirstOrDefault(sp => sp.PrescriptionId == prescriptionId);

            if (prescription != null)
            {
                // Update the Dispense field to "Received"
                prescription.Dispense = "Received";
                _context.SaveChanges();
            }

            // Redirect back to the NurseDispensedAlert page after updating
            return RedirectToAction("NurseDispensedAlert");
        }
    }
}

