using Hospital.Data;
using Hospital.Models;
using Hospital.ModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
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

            // Fetch administered medications for the patient (empty list if not found)
            var administeredMedications = await _context.AdministerMedication
                .Where(m => m.Patient_Id == patientId)
                .OrderByDescending(m => m.AdministerMedicationTime) // Order by time, latest first
                .ToListAsync();

            // Construct the model with vitals and other details
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
                Weight = vitals?.Weight ?? null,
                Height = vitals?.Height ?? null,
                BMI = vitals?.BMI ?? null,
                Temperature = vitals?.Tempreture ?? null,
                BloodPressure = vitals?.BloodPressure ?? null,
                Pulse = vitals?.Pulse ?? null,
                Respiratory = vitals?.Respiratory ?? null,
                BloodOxygen = vitals?.BloodOxygen ?? null,
                BloodGlucoseLevel = vitals?.BloodGlucoseLevel ?? null,
                VitalTime = vitals?.VitalTime ?? null,
                Allergies = allergies,
                CurrentMedications = currentMedications,
                MedicalConditions = medicalConditions,
                AdministeredMedications = administeredMedications // New property for administered medications
            };

            return View(model);
        }


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
                    BMI = v.BMI,
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

        // GET: Nurse/ViewPatients
        public async Task<IActionResult> NurseViewBookedPatient()
        {
            var bookedPatients = await _context.BookingSurgery
                .Include(b => b.Patient)
                .ToListAsync();

            return View(bookedPatients);
        }


        public IActionResult NurseDischargePatients()
        {
            // Fetch the list of patients and create a concatenated display name, then order alphabetically by full name
            var patients = _context.Patients
                .Select(p => new
                {
                    PatientIDNumber = p.PatientIDNumber,
                    FullName = p.PatientName + " " + p.PatientSurname
                })
                .OrderBy(p => p.FullName)  // Sort patients by FullName alphabetically
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
            // Fetch the provinces and sort them alphabetically
            var provinces = _context.Province.OrderBy(p => p.ProvinceName).ToList();

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
                // Fetch the province, town, and suburb, and order them alphabetically
                var province = _context.Province
                    .FirstOrDefault(p => p.ProvinceId == int.Parse(Request.Form["ProvinceId"]));

                var town = _context.Town
                    .Where(t => t.ProvinceId == province.ProvinceId)
                    .OrderBy(t => t.TownName)  // Sort towns alphabetically
                    .FirstOrDefault(t => t.TownId == int.Parse(Request.Form["TownId"]));

                var suburb = _context.Suburb
                    .Where(s => s.TownId == town.TownId)
                    .OrderBy(s => s.SuburbName)  // Sort suburbs alphabetically
                    .FirstOrDefault(s => s.SuburbId == int.Parse(Request.Form["SuburbId"]));

                var postalCode = Request.Form["PostalCode"];

                // Concatenate the address components with commas as a delimiter
                model.PatientAddress = $"{model.PatientAddress}, {province?.ProvinceName}, {town?.TownName}, {suburb?.SuburbName}, {postalCode}".Trim(',').Replace(",,", ",");

                // Save the new patient
                TempData["SuccessMessage"] = "Patient added successfully.";
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("NurseViewPatients");
            }

            // Repopulate the provinces list in case of validation failure
            ViewBag.Provinces = _context.Province.OrderBy(p => p.ProvinceName).ToList(); // Ensure provinces are sorted alphabetically
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
        public async Task<IActionResult> NurseAddPatientVital()
        {
            // Retrieve PatientId from TempData
            var patientId = TempData["PatientId"]?.ToString();

            // Check if PatientId is missing
            if (string.IsNullOrEmpty(patientId))
            {
                TempData["ErrorMessage"] = "Patient ID is required.";
                return RedirectToAction("SearchPatient"); // Redirect to a search page or an appropriate view
            }

            // Retrieve patient details using the PatientId
            var patient = await _context.Patients
                .Where(p => p.PatientIDNumber == patientId)
                .Select(p => new
                {
                    p.PatientName,
                    p.PatientSurname,
                    p.PatientEmailAddress
                })
                .FirstOrDefaultAsync();

            // If no patient found, return an error
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("SearchPatient");
            }

            // Set patient full name and email in ViewBag
            ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
            ViewBag.PatientEmail = patient.PatientEmailAddress;

            // Pass PatientId using ViewBag to maintain the context
            ViewBag.PatientId = patientId;

            // Return the view
            return View();
        }


        // POST: Nurse/AddPatientVital
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddPatientVital(PatientVital model)
        {
            // If PatientId is missing in the submitted model, attempt to retrieve it from TempData
            if (string.IsNullOrEmpty(model.PatientId) && TempData["PatientId"] != null)
            {
                model.PatientId = TempData["PatientId"].ToString();
            }

            // Store PatientId in TempData to persist it across requests, but don't overwrite it if already set.
            if (string.IsNullOrEmpty(model.PatientId))
            {
                TempData["ErrorMessage"] = "Patient ID is missing.";
                return RedirectToAction("NurseAddPatientVital");
            }
            else
            {
                TempData["PatientId"] = model.PatientId; // Store PatientId in TempData to use in subsequent requests
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Save the model to the database
                    _context.Add(model);
                    await _context.SaveChangesAsync();

                    // Check for concerning vitals and send an email if necessary
                    if (IsConcerningVital(model, out string message))
                    {
                        await SendConcerningVitalsEmail(model, message);
                    }

                    TempData["SuccessMessage"] = "Patient Vitals added successfully.";

                    // Redirect to the Add Current Medication form
                    return RedirectToAction("NurseAddCurrentMedication");
                }
                catch (Exception ex)
                {
                    // Log exception
                    Console.WriteLine(ex.Message);
                    ModelState.AddModelError("", "An error occurred while saving the data.");
                }
            }

            // Retrieve Patient details for repopulating the ViewBag if ModelState is invalid
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == model.PatientId);
            if (patient != null)
            {
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
            }

            // Repopulate the ViewBag with PatientId
            ViewBag.PatientId = model.PatientId;

            return View(model);
        }


        // Helper method to determine if vitals are concerning
        private bool IsConcerningVital(PatientVital model, out string message)
        {
            message = string.Empty;
            var (systolic, diastolic) = model.GetBloodPressureValues();

            // Track individual vital messages
            var concerningVitals = new List<string>();
            if (systolic > 140 || diastolic > 90)
            {
                concerningVitals.Add($"<strong>Blood Pressure is high:</strong> {model.BloodPressure}.");
            }

            if (model.Pulse < 60 || model.Pulse > 100)
            {
                concerningVitals.Add($"<strong>Heart Rate is abnormal:</strong> {model.Pulse}.");
            }

            if (model.Tempreture < 36.1m || model.Tempreture > 37.2m)
            {
                concerningVitals.Add($"<strong>Body Temperature is abnormal:</strong> {model.Tempreture}.");
            }


            if (concerningVitals.Count == 0)
            {
                return false; // No concerning vitals
            }

            // Construct the message as a bulleted list
            if (concerningVitals.Count > 1)
            {
                message = "Multiple concerning vitals detected:<ul style='padding-left: 20px;  font-weight: bold;'>";
                foreach (var vital in concerningVitals)
                {
                    message += $"<li>{vital}</li>";
                }
                message += "</ul>";
            }
            else
            {
                message = concerningVitals[0]; // Only one vital is concerning
            }

            return true;
        }


        // Method to send an email for concerning vitals
        private async Task SendConcerningVitalsEmail(PatientVital vital, string message)
        {
            try
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PatientIDNumber == vital.PatientId);

                if (patient == null)
                {
                    Console.WriteLine("Patient not found. Cannot send email.");
                    return;
                }

                var fromAddress = new MailAddress("kitadavids@gmail.com", "Northside Hospital");
                var toAddress = new MailAddress("nicky.mostert@nmmu.ac.za", "Admin");
                const string fromPassword = "nhjj efnx mjpv okee"; // Secure this in production

                var smtpClient = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = fromAddress,
                    Subject = "Concerning Vital Alert",
                    Body = $@"
<h2 style=""color: red; text-align: center; text-decoration: underline;"">Concerning Vital Detected</h2>

<p style=""text-align: left; margin-top: 20px; color: black;"">
    Patient Information:
</p>

<p style=""text-align: left; margin-bottom: 10px;  font-weight: bold; color: black;"">
    <strong>Patient Name:</strong> {patient.PatientName} {patient.PatientSurname}
</p>
<p style=""text-align: left; margin-bottom: 10px; font-weight: bold; color: black;"">
    <strong>Patient ID:</strong> {vital.PatientId}
</p>
<p style=""text-align: left; margin-bottom: 10px; font-weight: bold; color: black;"">
    <strong>Time:</strong> {vital.VitalTime}
</p>

<h3 style=""text-align: left; margin-bottom: 10px; font-weight: bold; color: red; text-align: center; text-decoration: underline;"">
    <strong>Details:</strong>
</h3>
{message}

<p style=""text-align: left; margin-top: 20px; font-weight: bold; color: red;"">
    Please review the patient's condition immediately.
</p>",
                    IsBodyHtml = true

                };

                mailMessage.To.Add(toAddress);
                await smtpClient.SendMailAsync(mailMessage);

                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }


        // GET: Nurse/NurseAddPatientVital2
        public IActionResult NurseAddPatientVital2(string id)
        {
            // Retrieve the patient details using the provided ID
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == id);

            if (patient != null)
            {
                // Pass the patient's full name and ID to the view
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
                ViewBag.PatientId = id;
            }
            else
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("ViewNurseAdmitPatients");
            }

            return View();
        }

        // POST: Nurse/NurseAddPatientVital2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddPatientVital2(PatientVital model)
        {
            // If PatientId is missing in the submitted model, attempt to retrieve it from TempData
            if (string.IsNullOrEmpty(model.PatientId) && TempData["PatientId"] != null)
            {
                model.PatientId = TempData["PatientId"].ToString();
            }

            // Store PatientId in TempData to persist it across requests, but don't overwrite it if already set.
            if (string.IsNullOrEmpty(model.PatientId))
            {
                TempData["ErrorMessage"] = "Patient ID is missing.";
                return RedirectToAction("NurseAddPatientVital2");
            }
            else
            {
                TempData["PatientId"] = model.PatientId; // Store PatientId in TempData to use in subsequent requests
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Save the model to the database
                    _context.Add(model);
                    await _context.SaveChangesAsync();

                    // Check for concerning vitals and send an email if necessary
                    if (IsConcerningVital(model, out string message))
                    {
                        await SendConcerningVitalsEmail(model, message);
                    }

                    TempData["SuccessMessage"] = "Patient Vitals added successfully.";
                    return RedirectToAction("NurseViewAdmitPatients");
                }
                catch (Exception ex)
                {
                    // Log exception
                    Console.WriteLine(ex.Message);
                    ModelState.AddModelError("", "An error occurred while saving the data.");
                }
            }

            // Retrieve Patient details for repopulating the ViewBag if ModelState is invalid
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == model.PatientId);
            if (patient != null)
            {
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
            }

            // Repopulate the ViewBag with PatientId
            ViewBag.PatientId = model.PatientId;

            return View(model);
        }



        // GET: Nurse/ViewPatients
        public async Task<IActionResult> NurseViewPatientVital()
        {
            var patientVitals = await _context.PatientVital
            .Include(v => v.Patient) // Include Patient data
            .ToListAsync();

            return View(patientVitals);
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
                    // Retrieve the patient's full name from the database based on PatientId
                    var patient = _context.Patients
                                          .Where(p => p.PatientIDNumber == vital.PatientId)
                                          .Select(p => new
                                          {
                                              FullName = p.PatientName + " " + p.PatientSurname
                                          })
                                          .FirstOrDefault();

                    // Ensure patient is found
                    var patientName = patient?.FullName ?? "Unknown Patient";


                    // Define colors for BMI, Temperature, Blood Pressure, Blood Glucose, and Oxygen saturation
                    string bmiColor = (vital.BMI < 18 || vital.BMI > 25) ? "color:#FF0000;" : "color:#808080;";
                    string temperatureColor = (vital.Tempreture > 37 || vital.Tempreture < 34) ? "color:#FF0000;" : "color:#808080;";
                    string bloodPressureColor = (IsHighBloodPressure(vital.BloodPressure) || IsLowBloodPressure(vital.BloodPressure)) ? "color:#FF0000;" : "color:#808080;";
                    string glucoseColor = (vital.BloodGlucoseLevel > 120) ? "color:#FF0000;" : "color:#808080;";
                    string oxygenColor = (vital.BloodOxygen < 90) ? "color:#FF0000;" : "color:#808080;";

                    // Append the vital details with color checks
                    vitalDetailsBuilder.Append($"<li><strong>Patient Name:</strong> {patientName}<br />" +
                                               $"<strong>Patient ID:</strong> {vital.PatientId}<br />" +
                                               $"<strong>Weight:</strong> {vital.Weight} kg<br />" +
                                               $"<strong>Height:</strong> {vital.Height} cm<br />" +
                                               $"<strong style=\"{bmiColor}\">BMI:</strong> {vital.BMI} <br />" +
                                               $"<strong style=\"{temperatureColor}\">Temperature:</strong> {vital.Tempreture} °C<br />" +
                                               $"<strong style=\"{bloodPressureColor}\">Blood Pressure:</strong> {vital.BloodPressure} mmHg<br />" +
                                               $"<strong>Pulse:</strong> {vital.Pulse} bpm<br />" +
                                               $"<strong>Respiratory Rate:</strong> {vital.Respiratory} breaths/min<br />" +
                                               $"<strong style=\"{oxygenColor}\">Blood Oxygen:</strong> {vital.BloodOxygen} %<br />" +
                                               $"<strong style=\"{glucoseColor}\">Blood Glucose Level:</strong> {vital.BloodGlucoseLevel} mg/dL<br />" +
                                               $"<strong>Vital Time:</strong> {vital.VitalTime?.ToString(@"hh\:mm")}<br />" +
                                               $"<br />");


    }

                vitalDetailsBuilder.Append("</ul>");

                // Define the email message
                var fromAddress = new MailAddress("kitadavids@gmail.com", "Nurse");
                //var toAddress = new MailAddress("nicky.mostert@mandela.ac.za", "Surgeon");
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
                    Subject = "Northside Hospital - Critical Patient Vital Alert",
                    Body = $@"
    <html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                line-height: 1.6;
            }}
            h3 {{
                color: #d9534f; /* Red color to highlight urgency */
            }}
            p {{
                font-size: 14px;
                color: #333;
            }}
            ul {{
                padding: 0;
                list-style-type: none;
            }}
            li {{
                margin-bottom: 15px;
                font-size: 14px;
            }}
            strong {{
                color: #5a5a5a;
            }}
        </style>
    </head>
    <body>
        <h3>Patient Vitals Alert</h3>
        <p>Attention required: The following patient vitals are outside normal ranges and require your <strong>IMMEDIATE ATTENTION</strong>:</p>
        <ul>
            {vitalDetailsBuilder}
        </ul>
        <p>Please prioritize this matter and address it as soon as possible.</p>
        <p>Best regards,<br />Northside Hospital Alert System</p>
    </body>
    </html>",
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
        // Helper functions for Blood Pressure Checks
        private bool IsHighBloodPressure(string bloodPressure)
        {
            if (string.IsNullOrEmpty(bloodPressure)) return false;
            var parts = bloodPressure.Split('/');
            if (parts.Length == 2)
            {
                int systolic = int.Parse(parts[0]);
                return systolic > 140;
            }
            return false;
        }

        private bool IsLowBloodPressure(string bloodPressure)
        {
            if (string.IsNullOrEmpty(bloodPressure)) return false;
            var parts = bloodPressure.Split('/');
            if (parts.Length == 2)
            {
                int diastolic = int.Parse(parts[1]);
                return diastolic < 60;
            }
            return false;
        }
        // GET: Nurse/AddPatientAllergy
        [HttpGet]
        public IActionResult NurseAddPatientAllergy()
        {
            // Retrieve PatientId from TempData
            var patientId = TempData["PatientId"]?.ToString();

            // Retrieve patient details using the PatientId
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == patientId);

            if (patient != null)
            {
                // If the patient is found, set the full name to ViewBag
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
                ViewBag.PatientId = patientId;
            }
            else
            {
                // If the patient is not found, use a fallback name
                ViewBag.SelectedPatientName = "Unknown Patient";
            }

            // Populate Active Ingredient dropdown in alphabetical order
            ViewBag.ActiveId = new SelectList(
                _context.ActiveIngredient.OrderBy(a => a.IngredientName),
                "IngredientName",
                "IngredientName"
            );

            // Pass the PatientId to the view model
            return View(new PatientAllergy { PatientId = patientId });
        }

        // POST: Nurse/AddPatientAllergy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddPatientAllergy(PatientAllergy model)
        {
            // If PatientId is missing in the submitted model, attempt to retrieve it from TempData
            if (string.IsNullOrEmpty(model.PatientId) && TempData["PatientId"] != null)
            {
                model.PatientId = TempData["PatientId"].ToString();
            }

            // Store PatientId in TempData to persist it across requests
            if (string.IsNullOrEmpty(model.PatientId))
            {
                TempData["ErrorMessage"] = "Patient ID is missing.";
                return RedirectToAction("NurseAddPatientAllergy");
            }
            else
            {
                TempData["PatientId"] = model.PatientId; // Store PatientId in TempData to use in subsequent requests
            }

            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Patient allergies added successfully.";

                var patientId = model.PatientId;

                // Get the allergies from the posted data (split by newline)
                var allergies = model.Allergy?.Split("\n", StringSplitOptions.RemoveEmptyEntries);

                if (allergies != null)
                {
                    foreach (var allergy in allergies)
                    {
                        var existingAllergy = await _context.PatientAllergies
                            .FirstOrDefaultAsync(pa => pa.PatientId == model.PatientId &&
                                                       pa.Allergy.Trim().ToLower() == allergy.Trim().ToLower());

                        if (existingAllergy != null)
                        {
                            ModelState.AddModelError("", $"The allergy '{allergy.Trim()}' is already added for the patient.");
                        }
                        else
                        {
                            // Add the new allergy
                            var patientAllergy = new PatientAllergy
                            {
                                PatientId = model.PatientId,
                                Allergy = allergy.Trim()
                            };

                            _context.PatientAllergies.Add(patientAllergy);
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Allergies saved successfully!";
                }

                // Store the PatientId in TempData for the next request
                TempData["PatientId"] = patientId;

                // Redirect to the Next Page (e.g., Add Vital Information)
                return RedirectToAction("NurseViewAdmitPatients");
            }

            // Repopulate the dropdown list in case of validation failure
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == model.PatientId);
            if (patient != null)
            {
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
            }

            // Repopulate Active Ingredient dropdown in alphabetical order in case of validation failure
            ViewBag.ActiveId = new SelectList(
                _context.ActiveIngredient.OrderBy(a => a.IngredientName),  // Sort active ingredients alphabetically
                "IngredientName",
                "IngredientName", model.Allergy
            );

            return View(model);
        }

        // GET: Nurse/NurseAddPatientAllergy2
        [HttpGet]
        public IActionResult NurseAddPatientAllergy2(string id)
        {
            // Retrieve the patient details using the provided ID
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == id);

            if (patient != null)
            {
                // Pass the patient's full name and ID to the view
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
                ViewBag.PatientId = id;
            }
            else
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("NurseViewAdmitPatients");
            }

            // Populate Active Ingredient dropdown in alphabetical order
            ViewBag.ActiveId = new SelectList(
                _context.ActiveIngredient.OrderBy(a => a.IngredientName),
                "IngredientName",
                "IngredientName"
            );


            // Initialize the model with the PatientId
            return View(new PatientAllergy { PatientId = id });
        }


        // POST: Nurse/NurseAddPatientAllergy2/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddPatientAllergy2(PatientAllergy model)
        {
            // Repopulate patient data and dropdown in case of errors
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == model.PatientId);

            if (patient != null)
            {
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
                ViewBag.PatientId = model.PatientId;
            }
            else
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("NurseViewAdmitPatients");
            }

            // Populate Active Ingredient dropdown in alphabetical order
            ViewBag.ActiveId = new SelectList(
                _context.ActiveIngredient.OrderBy(a => a.IngredientName),
                "IngredientName",
                "IngredientName"
            );

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the allergies from the submitted model
                    var allergies = model.Allergy?.Split("\n", StringSplitOptions.RemoveEmptyEntries);

                    if (allergies != null)
                    {
                        foreach (var allergy in allergies)
                        {
                            var existingAllergy = await _context.PatientAllergies
                                .FirstOrDefaultAsync(pa => pa.PatientId == model.PatientId &&
                                                           pa.Allergy.Trim().ToLower() == allergy.Trim().ToLower());

                            if (existingAllergy != null)
                            {
                                ModelState.AddModelError("", $"The allergy '{allergy.Trim()}' is already added for the patient.");
                            }
                            else
                            {
                                // Add the new allergy
                                var patientAllergy = new PatientAllergy
                                {
                                    PatientId = model.PatientId,
                                    Allergy = allergy.Trim()
                                };

                                _context.PatientAllergies.Add(patientAllergy);
                            }
                        }

                        if (ModelState.IsValid)
                        {
                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = "Allergies added successfully!";
                            return RedirectToAction("NurseViewAdmitPatients");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please provide at least one allergy.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    ModelState.AddModelError("", "An error occurred while saving the data.");
                }
            }

            // Return the view with the repopulated data
            return View(model);
        }

        // GET: PatientAllergy
        public async Task<IActionResult> NurseViewPatientAllergy()
        {
             var patientAllergies = await _context.PatientAllergies
            .Include(pa => pa.Patient)
            .ToListAsync();
            return View(patientAllergies);
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
            // Retrieve PatientId from TempData
            var patientId = TempData["PatientId"]?.ToString();

            // Retrieve patient details using the PatientId
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == patientId);

            if (patient != null)
            {
                // If the patient is found, set the full name to ViewBag
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
                ViewBag.PatientId = patientId;
            }
            else
            {
                // If the patient is not found, use a fallback name
                ViewBag.SelectedPatientName = "Unknown Patient";
            }

            // Populate ChronicMedication dropdown in alphabetical order
            ViewBag.ChronicMedicationId = new SelectList(
                _context.ChronicMedication.OrderBy(m => m.CMedicationName),
                "CMedicationName",
                "CMedicationName"
            );


            return View(new PatientCurrentMedication { PatientId = patientId });
        }

        // POST: Nurse/NurseAddCurrentMedication/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddCurrentMedication(PatientCurrentMedication model)
        {
            // If PatientId is missing in the submitted model, attempt to retrieve it from TempData
            if (string.IsNullOrEmpty(model.PatientId) && TempData["PatientId"] != null)
            {
                model.PatientId = TempData["PatientId"].ToString();
            }

            // Store PatientId in TempData to persist it across requests, but don't overwrite it if already set.
            if (string.IsNullOrEmpty(model.PatientId))
            {
                TempData["ErrorMessage"] = "Patient ID is missing.";
                return RedirectToAction("NurseAddCurrentMedication");
            }
            else
            {
                TempData["PatientId"] = model.PatientId; // Store PatientId in TempData to use in subsequent requests
            }

            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Medications added successfully.";
                var patientId = model.PatientId;

                // Get the medications from the posted data
                var medications = model.CurrentMedication?.Split("\n", StringSplitOptions.RemoveEmptyEntries);

                if (medications != null && medications.Any())
                {
                    var medicationsAlreadyAdded = new List<string>();

                    foreach (var medication in medications)
                    {
                        var existingMedication = await _context.PatientCurrentMedication
                            .FirstOrDefaultAsync(pm => pm.PatientId.Trim().ToLower() == patientId.Trim().ToLower() &&
                                                       pm.CurrentMedication.Trim().ToLower() == medication.Trim().ToLower());

                        if (existingMedication == null)
                        {
                            var patientMedication = new PatientCurrentMedication
                            {
                                PatientId = patientId,
                                CurrentMedication = medication.Trim()
                            };

                            _context.PatientCurrentMedication.Add(patientMedication);
                        }
                        else
                        {
                            medicationsAlreadyAdded.Add(medication);
                        }
                    }

                    if (medicationsAlreadyAdded.Any())
                    {
                        ViewBag.Error = "The following medications already exist for this patient: " + string.Join(", ", medicationsAlreadyAdded);
                        return View(model);
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Medications saved successfully!";
                    return RedirectToAction("NurseAddMedicalCondition");
                }
                else
                {
                    // If no medication is provided, show an error message but don't clear PatientId
                    ViewBag.Error = "Please select at least one medication before submitting.";
                    return View(model);
                }
            }
            else
            {
                // Repopulate the PatientId if ModelState is invalid
                TempData["PatientId"] = model.PatientId;

                var patientForError = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == model.PatientId);
                if (patientForError != null)
                {
                    ViewBag.SelectedPatientName = $"{patientForError.PatientName} {patientForError.PatientSurname}";
                }

                // Repopulate PatientId dropdown with FullName for patient selection in case of validation failure
                ViewBag.PatientId = new SelectList(_context.Patients
                    .OrderBy(p => p.PatientName + " " + p.PatientSurname),  // Sort patients alphabetically
                    "PatientIDNumber", "PatientName", model.PatientId);

                // Repopulate ChronicMedication dropdown in alphabetical order
                ViewBag.ChronicMedicationId = new SelectList(
                    _context.ChronicMedication.OrderBy(m => m.CMedicationName),  // Sort medications alphabetically
                    "CMedicationName", "CMedicationName", model.CurrentMedication
                );

                return View(model);
            }
        }

        // GET: NurseAddCurrentMedication2
        [HttpGet]
        public IActionResult NurseAddCurrentMedication2(string id)
        {
            // Retrieve the patient details using the provided ID
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == id);

            if (patient != null)
            {
                // Pass the patient's full name and ID to the view
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
                ViewBag.PatientId = id;
            }
            else
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("NurseViewAdmitPatients");
            }

            // Populate ChronicMedication dropdown in alphabetical order
            ViewBag.ChronicMedicationId = new SelectList(
                _context.ChronicMedication.OrderBy(m => m.CMedicationName),
                "CMedicationName",
                "CMedicationName"
            );

            // Initialize the model with the PatientId
            return View(new PatientCurrentMedication { PatientId = id });
        }

        // POST: Nurse/NurseAddCurrentMedication2/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddCurrentMedication2(PatientCurrentMedication model)
        {
            // Repopulate patient data and dropdown in case of errors
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == model.PatientId);

            if (patient != null)
            {
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
                ViewBag.PatientId = model.PatientId;
            }
            else
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("NurseViewAdmitPatients");
            }

            // Populate ChronicMedication dropdown in alphabetical order
            ViewBag.ChronicMedicationId = new SelectList(
                _context.ChronicMedication.OrderBy(m => m.CMedicationName),
                "CMedicationName",
                "CMedicationName"
            );

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if the medication already exists for the patient
                    var existingMedication = await _context.PatientCurrentMedication
                        .FirstOrDefaultAsync(pm => pm.PatientId == model.PatientId &&
                                                   pm.CurrentMedication.Trim().ToLower() == model.CurrentMedication.Trim().ToLower());

                    if (existingMedication != null)
                    {
                        ModelState.AddModelError("", "This medication is already added for the patient.");
                    }
                    else
                    {
                        // Add the new medication
                        _context.Add(model);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Medication added successfully.";
                        return RedirectToAction("NurseViewAdmitPatients");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    ModelState.AddModelError("", "An error occurred while saving the data.");
                }
            }

            // Return the view with proper data repopulated
            return View(model);
        }

        // GET: Nurse/ViewPatientCurrentMedication
        public async Task<IActionResult> NurseViewPatientCurrentMedication()
        {
            var patientMedications = await _context.PatientCurrentMedication
                .Include(pcm => pcm.Patient) // Include patient data
                .ToListAsync();

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
        [HttpGet]
        public IActionResult NurseAddMedicalCondition()
        {
            // Retrieve PatientId from TempData
            var patientId = TempData["PatientId"]?.ToString();

            if (string.IsNullOrEmpty(patientId))
            {
                // If PatientId is not found in TempData, display "Unknown Patient"
                ViewBag.SelectedPatientName = "Unknown Patient";
                ViewBag.PatientId = null;
            }
            else
            {
                var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == patientId);

                if (patient != null)
                {
                    // If the patient is found, set the full name to ViewBag
                    ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
                    ViewBag.PatientId = patientId;
                }
                else
                {
                    ViewBag.SelectedPatientName = "Unknown Patient";
                    ViewBag.PatientId = null; // In case the patient is not found in the database
                }
            }

            // Populate Medical Conditions dropdown in alphabetical order
            var medicalConditions = _context.ChronicCondition
                .OrderBy(c => c.Diagnosis)  // Sort the conditions alphabetically
                .Select(c => new SelectListItem
                {
                    Text = c.Diagnosis,
                    Value = c.Diagnosis
                })
                .ToList();

            ViewBag.MedicalConditions = medicalConditions;


            return View(new PatientMedicalCondition { PatientId = patientId });
        }

        // POST: Nurse/AddPatientMedicalCondition
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddMedicalCondition(PatientMedicalCondition model)
        {
            // If PatientId is missing in the submitted model, attempt to retrieve it from TempData
            if (string.IsNullOrEmpty(model.PatientId) && TempData["PatientId"] != null)
            {
                model.PatientId = TempData["PatientId"].ToString();
            }

            // Store PatientId in TempData to persist it across requests, but don't overwrite it if already set.
            if (string.IsNullOrEmpty(model.PatientId))
            {
                TempData["ErrorMessage"] = "Patient ID is missing.";
                return RedirectToAction("NurseAddMedicalCondition");
            }
            else
            {
                TempData["PatientId"] = model.PatientId; // Store PatientId in TempData to use in subsequent requests
            }

            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Medical conditions added successfully.";

                var patientId = model.PatientId;

                // Get the medical conditions from the posted data (split by newline)
                var conditions = model.MedicalCondition?.Split("\n", StringSplitOptions.RemoveEmptyEntries);

                if (conditions != null)
                {
                    foreach (var condition in conditions)
                    {
                        var existingCondition = await _context.PatientMedicalCondition
                            .FirstOrDefaultAsync(pm => pm.PatientId.Trim().ToLower() == model.PatientId.Trim().ToLower() &&
                                                       pm.MedicalCondition.Trim().ToLower() == condition.Trim().ToLower());

                        if (existingCondition != null)
                        {
                            // Add an error message for the duplicate condition
                            ModelState.AddModelError("", $"The medical condition '{condition.Trim()}' is already added for the patient.");
                        }
                        else
                        {
                            // Add the new medical condition
                            var patientCondition = new PatientMedicalCondition
                            {
                                PatientId = model.PatientId,
                                MedicalCondition = condition.Trim()
                            };

                            _context.PatientMedicalCondition.Add(patientCondition);
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Medical conditions saved successfully!";
                }

                // Store the PatientId in TempData for the next request
                TempData["PatientId"] = patientId;

                // Redirect to the next page (e.g., add allergies)
                return RedirectToAction("NurseAddPatientAllergy");
            }

            // Repopulate the ViewBag if the model state is invalid
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == model.PatientId);
            if (patient != null)
            {
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
            }

            // Populate Medical Conditions dropdown in alphabetical order
            ViewBag.MedicalConditions = _context.ChronicCondition
                .OrderBy(c => c.Diagnosis)  // Sort the conditions alphabetically
                .Select(c => new SelectListItem
                {
                    Text = c.Diagnosis,
                    Value = c.Diagnosis
                })
                .ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult NurseAddMedicalCondition2(string id)
        {
            // Retrieve the patient details using the provided ID
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == id);

            if (patient != null)
            {
                // Pass the patient's full name and ID to the view
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
                ViewBag.PatientId = id;
            }
            else
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("NurseViewAdmitPatients");
            }
            // Populate Medical Conditions dropdown in alphabetical order
            var medicalConditions = _context.ChronicCondition
                .OrderBy(c => c.Diagnosis)  // Sort the conditions alphabetically
                .Select(c => new SelectListItem
                {
                    Text = c.Diagnosis,
                    Value = c.Diagnosis
                })
                .ToList();

            ViewBag.MedicalConditions = medicalConditions;


            // Ensure the model initializes with the patient ID
            return View(new PatientMedicalCondition { PatientId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddMedicalCondition2(PatientMedicalCondition model)
        {
            // Repopulate the dropdown and patient details in all cases
            var patient = _context.Patients.FirstOrDefault(p => p.PatientIDNumber == model.PatientId);

            if (patient != null)
            {
                ViewBag.SelectedPatientName = $"{patient.PatientName} {patient.PatientSurname}";
                ViewBag.PatientId = model.PatientId;
            }
            else
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("NurseViewAdmitPatients");
            }

            // Populate Medical Conditions dropdown in alphabetical order
            var medicalConditions = _context.ChronicCondition
                .OrderBy(c => c.Diagnosis)  // Sort the conditions alphabetically
                .Select(c => new SelectListItem
                {
                    Text = c.Diagnosis,
                    Value = c.Diagnosis
                })
                .ToList();

            ViewBag.MedicalConditions = medicalConditions;

            // Proceed with saving if the model is valid
            if (ModelState.IsValid)
            {
                try
                {
                    var existingCondition = await _context.PatientMedicalCondition
                        .FirstOrDefaultAsync(pm => pm.PatientId == model.PatientId &&
                                                   pm.MedicalCondition.Trim().ToLower() == model.MedicalCondition.Trim().ToLower());

                    if (existingCondition != null)
                    {
                        ModelState.AddModelError("", "This medical condition is already added for the patient.");
                    }
                    else
                    {
                        // Add the new medical condition
                        _context.Add(model);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Medical condition added successfully.";
                        return RedirectToAction("NurseViewAdmitPatients");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    ModelState.AddModelError("", "An error occurred while saving the data.");
                }
            }

            // Return the view with proper repopulated data
            return View(model);
        }



        // GET: 
        public async Task<IActionResult> NurseViewPatientMedicalCondition()
        {
            var patientMedicalConditions = await _context.PatientMedicalCondition
            .Include(pmc => pmc.Patient)
            .ToListAsync();

            return View(patientMedicalConditions);
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

            // Group prescriptions by PrescribedID
            var dispensedMedications = _context.SurgeonPrescription
                .Where(sp => sp.Dispense == "Dispense")
                .GroupBy(sp => sp.PrescribedID)  // Group by PrescriptionId
                .Select(g => new NurseDispensedAlertViewModel
                {
                    Prescriptions = g.ToList(),  // Collect all prescriptions in the group
                    AdministeredMedications = _context.AdministerMedication
                        .Where(am => am.Patient_Id == g.FirstOrDefault().PatientIdnumber)  // Match on Patient_Id
                        .ToList()
                })
                .ToList();

            return View(dispensedMedications);
        }



        [HttpGet]
        public IActionResult NurseDispensedDetails(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                TempData["ErrorMessage"] = "Patient ID is missing.";
                return RedirectToAction("ErrorPage"); // Or another appropriate action
            }

            var model = new NurseDispensedDetailsViewModel
            {
                PatientId = patientId,
                Prescriptions = _context.SurgeonPrescription
                                        .Where(p => p.PatientIdnumber == patientId)
                                        .ToList(),
                AdministeredMedications = _context.AdministerMedication
                                                 .Where(a => a.Patient_Id == patientId)
                                                 .ToList()
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ReceiveMedication(
            int[] selectedMedications, // Indices of selected medications
            int[] prescriptionIds,
            string patientId,
            int[] medicationIds,
            string[] medicationNames,
            string[] dosageForms,
            int[] quantitiesToAdminister)
        {
            if (selectedMedications == null || selectedMedications.Length == 0 || string.IsNullOrEmpty(patientId))
            {
                TempData["ErrorMessage"] = "No medications were selected. Please select medications to administer.";
                return RedirectToAction("NurseDispensedDetails", new { patientId });
            }

            var successMessages = new List<string>();
            var errorMessages = new List<string>();
            var medicationNamesList = new List<string>(); // To store the medication names

            foreach (var index in selectedMedications)
            {
                var prescriptionId = prescriptionIds[index];
                var medicationId = medicationIds[index];
                var medicationName = medicationNames[index];
                var dosageForm = dosageForms[index];
                var quantityToAdminister = quantitiesToAdminister[index];

                // Retrieve the prescription
                var prescription = _context.SurgeonPrescription.FirstOrDefault(sp => sp.PrescriptionId == prescriptionId);

                if (prescription != null)
                {
                    var totalAdministered = _context.AdministerMedication
                        .Where(am => am.MedicationId == medicationId && am.Patient_Id == patientId)
                        .Sum(am => am.Quantity);

                    var remainingQuantity = prescription.Quantity - totalAdministered;

                    if (quantityToAdminister > remainingQuantity)
                    {
                        errorMessages.Add($"Cannot administer {quantityToAdminister} of {medicationName}. Only {remainingQuantity} remaining to administer.");
                        continue; // Skip and process the next one
                    }

                    // Create a new AdministerMedication entry
                    var administerMedication = new AdministerMedication
                    {
                        Patient_Id = patientId,
                        ScriptDetails = $"Administered: {medicationName}",
                        MedicationId = medicationId,
                        DosageFormName = dosageForm,
                        Quantity = quantityToAdminister,
                        AdministerMedicationTime = DateTime.Now
                    };

                    _context.Add(administerMedication);
                    medicationNamesList.Add($"{quantityToAdminister} of {medicationName}"); // Add medication details to the list
                }
                else
                {
                    errorMessages.Add($"Prescription with ID {prescriptionId} for {medicationName} not found.");
                }
            }

            if (medicationNamesList.Any())
            {
                // Construct the success message by joining the list of medications with commas
                var successMessage = "Successfully administered " + string.Join(", ", medicationNamesList) + ".";

                TempData["SuccessMessage"] = successMessage;
            }

            if (errorMessages.Any())
            {
                // Handle the error messages similarly
                var errorMessage = string.Join("<br />", errorMessages.Where(m => !string.IsNullOrWhiteSpace(m)));
                TempData["ErrorMessage"] = errorMessage;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("NurseDispensedDetails", new { patientId });
        }


        // GET: Nurse/NurseAddAdministerMedication
        public IActionResult NurseAddAdministerMedication(string patientId, string patientName, string medicationName, int medicationId, string dosageForm, int quantity)
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

            // Populate ViewBag.PatientId with a list of patients sorted by FullName
            var patients = _context.Patients
                .Select(p => new
                {
                    PatientIDNumber = p.PatientIDNumber,
                    FullName = p.PatientName + " " + p.PatientSurname
                })
                .OrderBy(p => p.FullName)  // Sort patients alphabetically by FullName
                .ToList();

            ViewBag.PatientId = new SelectList(patients, "PatientIDNumber", "FullName");

            // Set today's date as the default for DateAssigned
            var model = new PatientsAdministration
            {
                DateAssigned = DateTime.Now // This will set the DateAssigned to today's date
            };

            return View(model);
        }

        // POST: Nurse/AddPatientsAdministration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NurseAdmitPatient(PatientsAdministration model)
        {
            if (ModelState.IsValid)
            {
                // Check if the patient is already admitted based on their PatientId
                var existingPatient = _context.PatientsAdministration
                    .FirstOrDefault(pa => pa.PatientId.Trim().ToLower() == model.PatientId.Trim().ToLower());

                if (existingPatient != null)
                {
                    // If the patient is already admitted, return an error message
                    ModelState.AddModelError("", "This patient is already admitted.");
                    TempData["SuccessMessage"] = null; // Do not show success message when patient is already admitted.
                }
                else
                {
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
                        TempData["SuccessMessage"] = "Patient Admitted successfully.";

                        // Store PatientId in TempData to be used later in the Patient Vital form
                        TempData["PatientId"] = model.PatientId;

                        // Redirect to the Patient Vital form after successful addition
                        return RedirectToAction("NurseAddPatientVital");
                    }
                    else
                    {
                        // If the administration record for this patient and bed already exists, return an error message
                        ModelState.AddModelError("", "This patient is already assigned to the specified bed.");
                        TempData["SuccessMessage"] = null; // Do not show success message when patient is already admitted.
                    }
                }
            }

            // Repopulate dropdown list in case of validation failure or patient already admitted
            var patients = _context.Patients
                .Select(p => new
                {
                    PatientIDNumber = p.PatientIDNumber,
                    FullName = p.PatientName + " " + p.PatientSurname
                })
                .OrderBy(p => p.FullName)  // Sort patients alphabetically by FullName
                .ToList();

            ViewBag.PatientId = new SelectList(patients, "PatientIDNumber", "FullName", model.PatientId);
            ViewBag.WardName = new SelectList(_context.Ward, "WardName", "WardName", model.PatientWard);

            return View(model);
        }


        // GET: Nurse/ViewPatientsAdministration
        public async Task<IActionResult> NurseViewAdmitPatients()
        {
            var patientsAdmin = await _context.PatientsAdministration
                                              .Include(pa => pa.Patient)
                                              .ToListAsync();
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



        //Displaying pdf info
        [HttpGet]
        public IActionResult NurseReports(DateTime? startDate, DateTime? endDate)
        {
            // Default date range if not provided
            if (!startDate.HasValue)
            {
                startDate = DateTime.Now.AddMonths(-1); // last month as default
            }

            if (!endDate.HasValue)
            {
                endDate = DateTime.Now;
            }

            // Fetch data using LINQ
            var filteredPrescriptions = (from sp in _context.SurgeonPrescription
                                         join s in _context.Surgeons on sp.SurgeonId equals s.SurgeonId
                                         where sp.DispenseDateTime >= startDate && sp.DispenseDateTime <= endDate
                                         select new PrescriptionViewModel
                                         {
                                             DispenseDateTime = sp.DispenseDateTime,
                                             PatientIDNumber = sp.PatientIdnumber,
                                             Patient = $"{sp.PatientName} {sp.PatientSurname}",
                                             ScriptBy = $"{s.Name} {s.Surname}",
                                             MedicationName = sp.MedicationName,
                                             Quantity = sp.Quantity,
                                             Dispense = sp.Dispense
                                         }).ToList();

            // Get summary of dispensed medications
            var medicineSummary = GetMedicineSummary(filteredPrescriptions);

            var model = new PrescriptionFilterViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Prescriptions = filteredPrescriptions,
                MedicineSummary = medicineSummary // Add the summary to the model
            };

            return View(model); // Return the filtered results to a view
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
                                             PatientIDNumber = sp.PatientIdnumber,
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
            pdfDocument.Info.Title = "Filtered Medications"; // Set the document title

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
            string reportTitle = "MEDICATION REPORT";
            XFont titleFont = new XFont("Arial", 20); // Font for the title
            XSize titleSize = gfx.MeasureString(reportTitle, titleFont); // Measure title size
            double xTitlePosition = (page.Width - titleSize.Width) / 2; // Calculate centered X position
            gfx.DrawString(reportTitle, titleFont, XBrushes.Black, new XPoint(xTitlePosition, 50)); // Draw title

            // Retrieve and display the pharmacist's name and surname for the report
            var pharmacistName = DisplayNameAndSurname.passUserName ?? "NURSE"; // Get pharmacist's name
            var pharmacistSurname = DisplayNameAndSurname.passUserSurname ?? ""; // Get pharmacist's surname

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
            string[] headers = { "DATE & TIME", "PATIENT", "MEDICATION", "QTY" }; // Column headers // Column headers

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
                    prescription.Patient,
                    prescription.MedicationName,
                    prescription.Quantity.ToString()
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
            gfx.DrawString($"TOTAL Patients: {totalDispensed}", font, XBrushes.Black, new XPoint(40, yPoint));
            yPoint += 20; // Move down for the next total


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

            return File(stream, "application/pdf", "Medication Report.pdf"); // Return the PDF as a file
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

        public IActionResult PatientInformation()
        {
            return View();
        }
    }
}
    


