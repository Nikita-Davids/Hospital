using Hospital.Data;
using Hospital.Models;
using Hospital.ModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Text;
using System.Net.Mail;
using System.Net;
using Hospital.ViewModels;
using System.Drawing.Printing;
using System.Drawing;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp;

namespace Hospital.Controllers
{
    public class NurseController(ApplicationDbContext dbContext) : Controller
    {
        ApplicationDbContext _context = dbContext;

        public async Task<IActionResult> PatientOverview(string patientId)
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


        //public async Task<IActionResult> NurseVitalAlert()
        //{
        //    // Retrieve the user's full name from TempData
        //    ViewBag.UserName = TempData["UserName"];

        //    // Retrieve patient vitals that are out of range
        //    var outOfRangeVitals = _context.PatientVital
        //        .Where(v => v.Tempreture > 37 || v.Tempreture < 34 || // Temperature out of range
        //                    v.Pulse > 100 || v.Pulse < 60 || // Pulse out of range
        //                    v.BloodOxygen < 95) // Blood oxygen out of range
        //        .Select(v => new PatientVital
        //        {
        //            PatientVitalId = v.PatientVitalId,
        //            PatientId = v.PatientId,
        //            Weight = v.Weight,
        //            Height = v.Height,
        //            Tempreture = v.Tempreture,
        //            BloodPressure = v.BloodPressure,
        //            Pulse = v.Pulse,
        //            Respiratory = v.Respiratory,
        //            BloodOxygen = v.BloodOxygen,
        //            BloodGlucoseLevel = v.BloodGlucoseLevel,
        //            VitalTime = v.VitalTime
        //        })
        //        .OrderByDescending(v => v.VitalTime)
        //        .ToList();

        //    // Check if there are out of range vitals
        //    if (outOfRangeVitals.Any())
        //    {
        //        // Send email alert for the out of range vitals
        //        await SendPatientVitalEmail(outOfRangeVitals);
        //    }
        //    else
        //    {
        //        Console.WriteLine("No out-of-range vitals found.");
        //    }

        //    // Pass the filtered list of patient vitals to the view
        //    return View(outOfRangeVitals);
        //}

        public async Task<IActionResult> NurseVitalAlert()
        {
            // Retrieve the user's full name from TempData
            ViewBag.UserName = TempData["UserName"];

            // Retrieve patient vitals that are out of range
            var outOfRangeVitals = _context.PatientVital
                .Where(v => v.Tempreture > 37 || v.Tempreture < 34 || // Temperature out of range
                            v.Pulse > 100 || v.Pulse < 60 || // Pulse out of range
                            v.BloodOxygen < 95) // Blood oxygen out of range
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

            // Pass the filtered list of patient vitals to the view
            return View(outOfRangeVitals);
        }
        [HttpPost]
        public async Task<IActionResult> SendVitalEmail(string patientId)
        {
            // Retrieve the vital data for the specific patient
            var vital = _context.PatientVital.FirstOrDefault(v => v.PatientId == patientId);

            if (vital != null)
            {
                // Send the email for this specific patient's vital data
                await SendPatientVitalEmail(new List<PatientVital> { vital });

                // Optionally, set a success message to display on the page after the email is sent
                TempData["SuccessMessage"] = "Email sent successfully!";
            }
            else
            {
                // Handle the case where the patient vital is not found
                TempData["ErrorMessage"] = "Error: Patient not found.";
            }

            // Redirect back to the NurseVitalAlert view
            return RedirectToAction("NurseVitalAlert");
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

        // GET: Nurse/EditDischargedPatient/5
        public async Task<IActionResult> NurseEditDischargedPatient(int id)
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

        // POST: Nurse/EditDischargedPatient/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseEditDischargedPatient(int id, DischargedPatient model)
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
                return RedirectToAction("NurseViewDischargedPatients");
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


        public IActionResult NurseViewDischargedPatients()
        {
            // Fetch all DischargedPatient entries
            var dischargedPatients = _context.DischargedPatients.ToList();
            return View(dischargedPatients);
        }


        [HttpGet]
        public JsonResult GetTownsByProvince(int provinceId)
        {
            var towns = _context.Town.Where(t => t.ProvinceId == provinceId).Select(t => new { t.TownId, t.TownName }).ToList();
            return Json(towns);
        }

        [HttpGet]
        public JsonResult GetSuburbsByTown(int townId)
        {
            var suburbs = _context.Suburb.Where(s => s.TownId == townId).Select(s => new { s.SuburbId, s.SuburbName }).ToList();
            return Json(suburbs);
        }

        [HttpGet]
        public JsonResult GetPostalCodeBySuburb(int suburbId)
        {
            var postalCode = _context.Suburb.Where(s => s.SuburbId == suburbId).Select(s => s.SuburbPostalCode).FirstOrDefault();
            return Json(postalCode);
        }

        // GET: Nurse/AddPatient
        public IActionResult NurseAddPatients()
        {
            var provinces = _context.Province.ToList();

            // Pass the provinces to the ViewBag
            ViewBag.Provinces = provinces;
            return View();
        }

        // POST: Nurse/AddPatient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddPatients(Patients model)
        {
            if (ModelState.IsValid)
            {
                // Fetch the names from the database based on the selected IDs
                var province = _context.Province.FirstOrDefault(p => p.ProvinceId == int.Parse(Request.Form["ProvinceId"]));
                var town = _context.Town.FirstOrDefault(t => t.TownId == int.Parse(Request.Form["TownId"]));
                var suburb = _context.Suburb.FirstOrDefault(s => s.SuburbId == int.Parse(Request.Form["SuburbId"]));
                var postalCode = Request.Form["PostalCode"];

                // Concatenate the address components with commas as a delimiter
                model.PatientAddress = $"{model.PatientAddress}, {province?.ProvinceName}, {town?.TownName}, {suburb?.SuburbName}, {postalCode}".Trim(',').Replace(",,", ",");



                TempData["SuccessMessage"] = "Patient added successfully.";
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("NurseAddPatients");
            }
           
            ViewBag.Provinces = _context.Province.ToList(); // Ensure provinces are loaded in case of an error
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






        private async Task SendPatientVitalEmail(List<PatientVital> selectedVitals)
        {
            // Check if there are any vitals to email about
            if (selectedVitals == null || !selectedVitals.Any())
            {
                Console.WriteLine("No patient vitals selected for email notification.");
                return;
            }

            try
            {
                // Build the vital details for the email body
                StringBuilder vitalDetailsBuilder = new StringBuilder("<ul>");

                foreach (var vital in selectedVitals)
                {
                    // Add patient vital details to the email body
                    vitalDetailsBuilder.Append($"<li>Patient ID: {vital.PatientId}<br />" +
                                                $"Weight: {vital.Weight} kg<br />" +
                                                $"Height: {vital.Height} cm<br />" +
                                                $"Temperature: {vital.Tempreture} °C<br />" +
                                                $"Blood Pressure: {vital.BloodPressure} mmHg<br />" +
                                                $"Pulse: {vital.Pulse} bpm<br />" +
                                                $"Respiratory Rate: {vital.Respiratory} breaths/min<br />" +
                                                $"Blood Oxygen: {vital.BloodOxygen} %<br />" +
                                                $"Blood Glucose Level: {vital.BloodGlucoseLevel} mg/dL<br />" +
                                                $"Vital Time: {vital.VitalTime?.ToString(@"hh\:mm")}<br /></li>");

                }

                vitalDetailsBuilder.Append("</ul>");

                // Define the email message
                var fromAddress = new MailAddress("kitadavids@gmail.com", "Nurse");
                var toAddress = new MailAddress("gabrielkojo77@gmail.com", "Surgeon");
                const string fromPassword = "nhjj efnx mjpv okee"; // Use environment variables or secure storage in production

                var smtpClient = new System.Net.Mail.SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = fromAddress,
                    Subject = "Northside Hospital - Patient Vital Alert",
                    Body = $@"
                            <h3>Patient Vitals Alert</h3>
                            <p>The following patient vitals require your IMMEDIATE ATTENTION:</p>
                            {vitalDetailsBuilder}
                            <p>please attend to the matter ASAP</p>",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toAddress);

                // Send the email
                await smtpClient.SendMailAsync(mailMessage);

                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                // Log email sending errors with more detail
                Console.WriteLine($"Error occurred while sending email: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
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
                    sp.MedicationId,
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










        // GET: Nurse/NurseAddAdministerMedication
        public IActionResult NurseAddAdministerMedication(string patientId, string patientName, string medicationName,int medicationId, string dosageForm, int quantity)
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

        // POST: Nurse/NurseAddAdministerMedication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddAdministerMedication(AdministerMedication model)
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

                    return RedirectToAction("NurseAddAdministerMedication"); // Redirect to the list of administered medications
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

        public IActionResult NurseViewAdministerMedication()
        {
            // Fetch all DischargedPatient entries
            var administerMedication = _context.AdministerMedication.ToList();
            return View(administerMedication);
        }

        public async Task<IActionResult> NurseEditAdministerMedication(int id)
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

        // POST: Nurse/EditAdministerMedication/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseEditAdministerMedication(int id, AdministerMedication model)
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
                return RedirectToAction("NurseViewAdministerMedication");
            }

            // Populate ViewBag for dropdown list again, if applicable
            ViewBag.MedicationId = new SelectList(await _context.Medication.ToListAsync(), "MedicationId", "MedicationName", model.MedicationId);

            return View(model);
        }

        private bool AdministerMedicationExists(int id)
        {
            return _context.AdministerMedication.Any(e => e.AdministerMedication_Id == id);
        }




        // GET: Nurse/AddPatientsAdministration
        [HttpGet]
        public IActionResult NurseAdmitPatient()
        {
            // Populate ViewBag with dropdown list of wards
            ViewBag.WardName = new SelectList(_context.Ward, "WardName", "WardName"); // Use WardName for both value and display

            // Populate ViewBag with dropdown lists for patients
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName");
            return View(new PatientsAdministration());
        }

        // POST: Nurse/AddPatientsAdministration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NurseAdmitPatient(PatientsAdministration model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Patient administration added successfully.";

                // Check if a patient administration record with the same patient and bed already exists
                var existingAdmin = _context.PatientsAdministration
                    .FirstOrDefault(pa => pa.PatientId.Trim().ToLower() == model.PatientId.Trim().ToLower() &&
                                          pa.PatientBed == model.PatientBed);

                if (existingAdmin == null)
                {
                    // Create a new PatientsAdministration entity
                    var patientsAdmin = new PatientsAdministration
                    {
                        PatientId = model.PatientId,
                        PatientWard = model.PatientWard,
                        PatientBed = model.PatientBed,
                       DateAssigned = model.DateAssigned
                    };

                    // Add the PatientsAdministration entity to the context
                    _context.PatientsAdministration.Add(patientsAdmin);
                    _context.SaveChanges(); // Save changes

                    // Redirect to the success page after successful addition
                    return RedirectToAction("NurseAddPatientsAdministration");
                }
                else
                {
                    // If the administration record for this patient and bed already exists, return an error message
                    ModelState.AddModelError("", "This patient is already assigned to the specified bed.");
                }
            }

            // Repopulate dropdown list in case of validation failure
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName", model.PatientId);
            return View(model);
        }
        // GET: Nurse/ViewPatientsAdministration
        public async Task<IActionResult> NurseViewAdmitPatients()
        {
            var patientsAdmin = await _context.PatientsAdministration.ToListAsync();
            return View(patientsAdmin);
        }
        // GET: Nurse/EditPatientsAdministration/5
        public async Task<IActionResult> NurseEditAdmitPatient(int id)
        {
            // Retrieve the patient administration record from the database using the provided ID
            var patientsAdmin = await _context.PatientsAdministration.FirstOrDefaultAsync(m => m.PatientsAdministration1 == id);

            // Check if the patient administration exists. If not, return a 404 Not Found response.
            if (patientsAdmin == null)
            {
                return NotFound();
            }

            // Populate ViewBag with a list of patients for the dropdown
            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientName", patientsAdmin.PatientId);

            // Return the view with the patient administration data to be edited
            return View(patientsAdmin);
        }

        // POST: Nurse/EditPatientsAdministration/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseEditAdmitPatient(int id, [Bind("PatientsAdministration1,PatientId,PatientWard,PatientBed,DateAssigned")] PatientsAdministration patientsAdmin)
        {
            if (id != patientsAdmin.PatientsAdministration1)
            {
                return NotFound();
            }

            // Check if a similar administration record exists for the same patient and bed (excluding current entry)
            if (_context.PatientsAdministration.Any(pa => pa.PatientId == patientsAdmin.PatientId && pa.PatientBed == patientsAdmin.PatientBed && pa.PatientsAdministration1 != id))
            {
                ModelState.AddModelError("PatientBed", "This patient is already assigned to the specified bed.");
                ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientName", patientsAdmin.PatientId);
                return View(patientsAdmin);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patientsAdmin);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientsAdministrationExists(patientsAdmin.PatientsAdministration1))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(NurseViewAdmitPatients));
            }

            ViewBag.PatientId = new SelectList(await _context.Patients.ToListAsync(), "PatientIDNumber", "PatientName", patientsAdmin.PatientId);
            return View(patientsAdmin);
        }

        // Helper method to check if a patient administration record exists in the database
        private bool PatientsAdministrationExists(int id)
        {
            return _context.PatientsAdministration.Any(e => e.PatientsAdministration1 == id);
        }


     

        private List<MedicineSummaryViewModel> GetMedicineSummary(List<PrescriptionViewModel> prescriptions)
        {
            return prescriptions
                .Where(p => p.Dispense == "Dispense") // Filter only dispensed prescriptions
                .GroupBy(p => p.MedicationName) // Group by medication name
                .Select(g => new MedicineSummaryViewModel
                {
                    MedicationName = g.Key,
                    TotalQuantity = g.Sum(p => p.Quantity) // Sum quantities for each medication
                })
                .ToList();
        }

        [HttpPost]
        public IActionResult ExportToPdf(DateTime? startDate, DateTime? endDate)
        {
            // Create a new instance of the database context to access data

            // Set default dates if not provided; defaults to the last month for startDate and now for endDate
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            // Retrieve filtered prescriptions based on the provided date range
            var filteredPrescriptions = (from sp in _context.SurgeonPrescription
                                         join s in _context.Surgeons on sp.SurgeonId equals s.SurgeonId
                                         where sp.DispenseDateTime >= startDate && sp.DispenseDateTime <= endDate
                                         select new PrescriptionViewModel
                                         {
                                             DispenseDateTime = sp.DispenseDateTime,
                                             PatientIDNumber = sp.PatientIdnumber.Trim(),
                                             Patient = $"{sp.PatientName} {sp.PatientSurname}",
                                             ScriptBy = $"{s.Name} {s.Surname}",
                                             MedicationName = sp.MedicationName,
                                             Quantity = sp.Quantity,
                                             Dispense = sp.Dispense
                                         }).ToList();

            // Get a summary of dispensed medications, grouping by medication name and summing quantities
            var medicineSummary = GetMedicineSummary(filteredPrescriptions);

            // Prepare the PDF document for export
            var pdfDocument = new PdfDocument();
            pdfDocument.Info.Title = "Filtered Prescriptions"; // Set the document title

            // Create a new A3 landscape page for the report
            int currentPage = 1; // Track the current page number
            PdfPage page = pdfDocument.AddPage();
            page.Size = PageSize.A3; // Set page size to A3 for more content space
            page.Orientation = PageOrientation.Landscape; // Set orientation to landscape
            XGraphics gfx = XGraphics.FromPdfPage(page); // Create graphics object to draw on the page
            XFont font = new XFont("Arial", 11); // Default font for general text
            XFont headerFont = new XFont("Arial", 12); // Font for headers
            XFont pageNumberFont = new XFont("Arial", 11); // Font for page numbers

            // Get the current date for report generation timestamp
            DateTime reportGeneratedDate = DateTime.Now;
            string formattedReportDate = reportGeneratedDate.ToString("d MMMM yyyy"); // Format the date

            // Draw the report title at the top of the page and center it
            string reportTitle = "DISPENSARY REPORT";
            XFont titleFont = new XFont("Arial", 20); // Font for the title
            XSize titleSize = gfx.MeasureString(reportTitle, titleFont); // Measure title size
            double xTitlePosition = (page.Width - titleSize.Width) / 2; // Calculate centered X position
            gfx.DrawString(reportTitle, titleFont, XBrushes.Black, new XPoint(xTitlePosition, 50)); // Draw title

            // Retrieve and display the pharmacist's name and surname for the report
            var pharmacistName = DisplayNameAndSurname.passUserName ?? "Unknown Name"; // Get pharmacist's name
            var pharmacistSurname = DisplayNameAndSurname.passUserSurname ?? "Unknown Surname"; // Get pharmacist's surname

            // Create a bold font for the pharmacist's name and surname
            XFont boldFont = new XFont("Arial", 12);
            string pharmacistFullName = $"{pharmacistName} {pharmacistSurname}"; // Combine names

            // Measure the width of the full name to center it
            XSize fullNameSize = gfx.MeasureString(pharmacistFullName, boldFont);
            double xFullNamePosition = (page.Width - fullNameSize.Width) / 2; // Center the full name

            // Draw the pharmacist's name and surname in bold at the center of the page
            gfx.DrawString(pharmacistFullName, boldFont, XBrushes.Black, new XPoint(xFullNamePosition, 80)); // Adjust Y position as needed

            // Draw the date range for the report on the left side
            gfx.DrawString($"Date Range: {startDate.Value.ToString("d MMMM yyyy")} - {endDate.Value.ToString("d MMMM yyyy")}", font, XBrushes.Black, new XPoint(40, 100));

            // Calculate position for "Report generated" text on the right-hand side
            string reportGeneratedText = $"Report Generated: {formattedReportDate}";
            XSize reportGeneratedTextSize = gfx.MeasureString(reportGeneratedText, font);
            double xPositionForReportGenerated = page.Width - reportGeneratedTextSize.Width - 40; // Align right with 40px margin

            // Draw the "Report generated" text on the right side
            gfx.DrawString(reportGeneratedText, font, XBrushes.Black, new XPoint(xPositionForReportGenerated, 100));

            // Add some space before the table by drawing an empty string
            gfx.DrawString("", font, XBrushes.Black, new XPoint(40, 100)); // Empty line for spacing

            // Define fixed column widths for the table
            float[] columnWidths = { 160, 160, 160, 160, 160, 160, 160 }; // Set widths for columns
            string[] headers = { "DATE", "PATIENT ID", "PATIENT", "SCRIPT BY", "MEDICATION", "QTY", "STATUS" }; // Column headers

            // Draw the headers for the table
            DrawTableRow(gfx, headers, headerFont, 120, columnWidths, true); // Call method to draw table row

            // Draw table rows for each prescription in the filtered list
            int yPoint = 140; // Start Y position for the first data row
            double rowHeight = 20; // Set height for each row
            foreach (var prescription in filteredPrescriptions)
            {
                // Prepare data for each row
                string[] rowData = {
                prescription.DispenseDateTime?.ToString("g") ?? "N/A", // Format dispense date
                prescription.PatientIDNumber,
                prescription.Patient,
                prescription.ScriptBy,
                prescription.MedicationName,
                prescription.Quantity.ToString(),
                prescription.Dispense
            };

                // Check if there is enough space for the next row
                if (yPoint + rowHeight > page.Height - 40) // Leave some space at the bottom
                {
                    // Draw page number before adding a new page
                    DrawPageNumber(gfx, pageNumberFont, currentPage, page);
                    currentPage++; // Increment page number

                    // Add a new page if needed
                    page = pdfDocument.AddPage(); // Create a new page
                    gfx = XGraphics.FromPdfPage(page); // Get graphics for the new page
                    yPoint = 40; // Reset y position for new page
                }

                // Draw the current row of data
                DrawTableRow(gfx, rowData, font, yPoint, columnWidths, false);
                yPoint += (int)rowHeight; // Move to the next row
            }

            // Calculate totals for dispensed and rejected scripts
            int totalDispensed = filteredPrescriptions.Count(p => p.Dispense == "Dispense"); // Count dispensed scripts
            int totalRejected = filteredPrescriptions.Count(p => p.Dispense == "Rejected"); // Count rejected scripts

            // Add some space before the totals
            yPoint += 20; // Space between the last row and totals

            // Draw the totals at the bottom of the report
            gfx.DrawString($"TOTAL SCRIPTS DISPENSED: {totalDispensed}", font, XBrushes.Black, new XPoint(40, yPoint));
            yPoint += 20; // Move down for the next total
            gfx.DrawString($"TOTAL SCRIPTS REJECTED: {totalRejected}", font, XBrushes.Black, new XPoint(40, yPoint));

            // Increase the space to create a break after the heading
            yPoint += 30; // Adjust this value to create more space after the heading

            // Add space before the medicine summary
            yPoint += 10; // Space before summary
            gfx.DrawString("SUMMARY PER MEDICINE:", new XFont("Verdana", 12), XBrushes.Black, new XPoint(40, yPoint)); // Draw summary title

            // Increase the space to create a break after the heading
            yPoint += 30; // Adjust this value to create more space after the heading

            // Draw summary table headers for the medicine summary
            string[] summaryHeaders = { "MEDICINE", "QTY DISPENSED" };
            DrawTableRow(gfx, summaryHeaders, headerFont, yPoint, new float[] { 140, 140 }, true); // Draw headers
            yPoint += 20; // Space before the first summary row

            // Draw summary table rows for each unique medicine
            foreach (var summaryItem in medicineSummary)
            {
                // Prepare data for each summary row
                string[] summaryRowData = {
                summaryItem.MedicationName,
                summaryItem.TotalQuantity.ToString()
            };

                // Check if there is enough space for the next summary row
                if (yPoint + rowHeight > page.Height - 40) // Leave some space at the bottom
                {
                    // Draw page number before adding a new page
                    DrawPageNumber(gfx, pageNumberFont, currentPage, page);
                    currentPage++; // Increment page number

                    // Add a new page if needed
                    page = pdfDocument.AddPage(); // Create a new page
                    gfx = XGraphics.FromPdfPage(page); // Get graphics for the new page
                    yPoint = 40; // Reset y position for new page
                }

                // Draw the current summary row
                DrawTableRow(gfx, summaryRowData, font, yPoint, new float[] { 140, 140 }, false);
                yPoint += (int)rowHeight; // Move to the next row
            }

            // Draw the final page number
            DrawPageNumber(gfx, pageNumberFont, currentPage, page);

            // Set the response content type and filename for the PDF download
            var stream = new MemoryStream();
            pdfDocument.Save(stream); // Save the document to the stream
            stream.Position = 0; // Reset stream position for reading

            return File(stream, "application/pdf", "Dispensary_Report.pdf"); // Return the PDF as a file
        }


        // Method to draw a table row in the PDF
        private void DrawTableRow(XGraphics gfx, string[] rowData, XFont font, int yPoint, float[] columnWidths, bool isHeader)
        {
            for (int i = 0; i < rowData.Length; i++)
            {
                float xPoint = 40 + i * columnWidths[i]; // Calculate X position for each column
                gfx.DrawString(rowData[i], font, XBrushes.Black, new XPoint(xPoint, yPoint + 5)); // Draw the cell content
                gfx.DrawRectangle(XPens.Black, xPoint, yPoint - 12, columnWidths[i], 20); // Draw the cell border
            }
        }

        // Method to draw the page number
        private void DrawPageNumber(XGraphics gfx, XFont font, int pageNumber, PdfPage page)
        {
            gfx.DrawString($"Page {pageNumber}", font, XBrushes.Black, new XPoint(page.Width - 50, page.Height - 30)); // Draw page number
        }
    }
}

