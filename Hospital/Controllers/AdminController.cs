using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hospital.Models;
using Hospital.ViewModels;
using System.Security.Policy;
using Hospital.Data;

namespace Hospital.Controllers
{
    public class AdminController(ApplicationDbContext dbContext) : Controller
    {
        ApplicationDbContext _context = dbContext;

        public IActionResult AdminViewHospital()
        {
            return View(_context.DayHospital);
        }
        public async Task<IActionResult> Index()
        {
            // Create a new instance of the NorthsideContext to interact with the database.

            // Create an instance of HealthcareStaffViewModel and populate its properties.
            var viewModel = new HealthcareStaffViewModel
            {
                // Retrieve the list of nurses from the database and assign it to the Nurses property of the view model.
                Nurses = await _context.Nurses.ToListAsync(),

                // Retrieve the list of pharmacists from the database and assign it to the Pharmacists property of the view model.
                Pharmacists = await _context.Pharmacists.ToListAsync(),

                // Retrieve the list of surgeons from the database and assign it to the Surgeons property of the view model.
                Surgeons = await _context.Surgeons.ToListAsync()
            };

            // Pass the populated view model to the view for rendering.
            return View(viewModel);
        }

        // GET: /Admin/RegisterHealthCareProfessionals
        [HttpGet]
        public IActionResult RegisterHealthCareProfessionals()
        {
            // Return the view with an empty view model
            return View(new RegisterHealthCareProfessionalViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterHealthCareProfessionals(RegisterHealthCareProfessionalViewModel model)
        {
            // Create a new instance of the NORTHSIDEHOSPITALContext to interact with the database.
            

            // Check if the model state is valid (i.e., all required fields are filled out correctly).
            if (ModelState.IsValid)
            {
                // Convert the input to lower case for case-insensitive comparison
                var email = model.EmailAddress.Trim().ToLower();
                var registrationNumber = model.HealthCouncilRegistrationNumber.Trim().ToLower();

                // Check for duplicate email and registration number
                bool emailExists = await _context.Nurses.AnyAsync(n => n.EmailAddress.Trim().ToLower() == email) ||
                                   await _context.Surgeons.AnyAsync(s => s.EmailAddress.Trim().ToLower() == email) ||
                                   await _context.Pharmacists.AnyAsync(p => p.EmailAddress.Trim().ToLower() == email);

                bool registrationNumberExists = await _context.Nurses.AnyAsync(n => n.HealthCouncilRegistrationNumber.Trim().ToLower() == registrationNumber) ||
                                                await _context.Surgeons.AnyAsync(s => s.HealthCouncilRegistrationNumber.Trim().ToLower() == registrationNumber) ||
                                                await _context.Pharmacists.AnyAsync(p => p.HealthCouncilRegistrationNumber.Trim().ToLower() == registrationNumber);

                if (emailExists)
                {
                    ModelState.AddModelError("EmailAddress", "A healthcare professional with this email address already exists.");
                }

                if (registrationNumberExists)
                {
                    ModelState.AddModelError("HealthCouncilRegistrationNumber", "A healthcare professional with this Health Council Registration Number already exists.");
                }

                // If there are validation errors, return the view with the current model
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Determine the role selected by the user and create an instance of the corresponding entity.
                switch (model.Role)
                {
                    case "Nurse":
                        // Create a new Nurse object with the provided details.
                        var nurse = new Nurse
                        {
                            Name = model.Name,
                            Surname = model.Surname,
                            ContactNumber = model.ContactNumber,
                            EmailAddress = model.EmailAddress,
                            HealthCouncilRegistrationNumber = model.HealthCouncilRegistrationNumber
                        };
                        // Add the Nurse object to the Nurses DbSet.
                        _context.Nurses.Add(nurse);
                        break;

                    case "Surgeon":
                        // Create a new Surgeon object with the provided details.
                        var surgeon = new Surgeon
                        {
                            Name = model.Name,
                            Surname = model.Surname,
                            ContactNumber = model.ContactNumber,
                            EmailAddress = model.EmailAddress,
                            HealthCouncilRegistrationNumber = model.HealthCouncilRegistrationNumber
                        };
                        // Add the Surgeon object to the Surgeons DbSet.
                        _context.Surgeons.Add(surgeon);
                        break;

                    case "Pharmacist":
                        // Create a new Pharmacist object with the provided details.
                        var pharmacist = new Pharmacist
                        {
                            Name = model.Name,
                            Surname = model.Surname,
                            ContactNumber = model.ContactNumber,
                            EmailAddress = model.EmailAddress,
                            HealthCouncilRegistrationNumber = model.HealthCouncilRegistrationNumber
                        };
                        // Add the Pharmacist object to the Pharmacists DbSet.
                        _context.Pharmacists.Add(pharmacist);
                        break;

                    default:
                        // If the selected role is not valid, add an error to the ModelState and return the view with the current model.
                        ModelState.AddModelError("", "Invalid role selected.");
                        return View(model);
                }

                // Save changes to the database asynchronously.
                await _context.SaveChangesAsync();

                // Redirect to the Index action method upon successful registration.
                return RedirectToAction("Index");
            }

            // If the model state is not valid, return the view with the current model to display validation errors.
            return View(model);
        }


       

        [HttpGet]
        public IActionResult AdminAddDosageForms()
        {
            var model = new DosageForm();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminAddDosageForms(DosageForm model)
        {
           

            if (ModelState.IsValid)
            {
                // Convert the input to lower case for case-insensitive comparison
                var dosageFormName = model.DosageFormName.Trim().ToLower();

                // Check if a dosage form with the same name already exists
                var existingDosageForm = _context.DosageForm
                    .FirstOrDefault(df => df.DosageFormName.Trim().ToLower() == dosageFormName);

                if (existingDosageForm == null)
                {
                    // Adding the dosage form entity to the context
                    _context.DosageForm.Add(model);
                    _context.SaveChanges(); // Save changes

                    // Redirect to the success page after successful addition
                    return RedirectToAction("AdminViewDosageForms");
                }
                else
                {
                    // If the dosage form with the same name already exists, return an error message
                    ModelState.AddModelError("DosageFormName", "Dosage form with the same name already exists.");
                }
            }

            // Return the view with the model in case of an error
            return View(model);
        }

        // GET: DosageForms/Edit/5
        public async Task<IActionResult> AdminEditDosageForms(int id)
        {
           
            // Retrieve the dosage form record from the database using the provided ID
            var dosageForm = await _context.DosageForm.FindAsync(id);

            // Check if the dosage form exists. If not, return a 404 Not Found response.
            if (dosageForm == null)
            {
                return NotFound();
            }

            // Return the view with the dosage form data to be edited
            return View(dosageForm);
        }

        // POST: DosageForms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminEditDosageForms(int id, [Bind("DosageFormID,DosageFormName")] DosageForm dosageForm)
        {
           
            // Create a new instance of the context to interact with the database
            if (id != dosageForm.DosageFormID)
            {
                // Check if the ID in the URL matches the ID of the dosage form being edited
                return NotFound();
            }

            // Check if the new dosage form name already exists in the database, excluding the current entry
            if (_context.DosageForm.Any(df => df.DosageFormName == dosageForm.DosageFormName && df.DosageFormID != id))
            {
                // Add a model error if the name already exists and return the view with the error
                ModelState.AddModelError("DosageFormName", "The dosage form name already exists.");
                return View(dosageForm);
            }

            // If the model state is valid, update the dosage form in the database
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the dosage form record in the database
                    _context.Update(dosageForm);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency issues if the record has been modified by another user
                    if (!DosageFormExists(dosageForm.DosageFormID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the Index action if the update was successful
                return RedirectToAction(nameof(AdminViewDosageForms));
            }

            // Return the view with validation errors if the model state is not valid
            return View(dosageForm);
        }

        // Helper method to check if a dosage form exists in the database
        private bool DosageFormExists(int id)
        {
           
            return _context.DosageForm.Any(e => e.DosageFormID == id);
        }

        // Action method for displaying the list of added dosage forms
        public IActionResult AdminViewDosageForms()
        {
            // Initialize the database context to access the Dosage Forms table
           

            // Retrieve all dosage forms records from the database and store them in a list
            List<DosageForm> temp = _context.DosageForm.ToList();

            // Pass the list of medications to the view for display
            return View(temp);
        }
        [HttpGet]
        public IActionResult AdminAddHospital()
        {
            var model = new DayHospital();
            return View(model);
        }

        [HttpPost]
        public IActionResult AdminAddHospital(DayHospital model)
        {
            if (ModelState.IsValid)
            {
                // Check if a hospital with the same name or email already exists
                var existingHospital = _context.DayHospital
                    .FirstOrDefault(h => h.HospitalName == model.HospitalName || h.EmailAddress == model.EmailAddress);

                if (existingHospital == null)
                {
                    // Creating the hospital entity
                    var dayhospital = new DayHospital
                    {
                        // No need to set HospitalId, it will be auto-generated
                        HospitalName = model.HospitalName,
                        Address = model.Address,
                        City = model.City,
                        Province = model.Province,
                        PostalCode = model.PostalCode,
                        ContactNumber = model.ContactNumber,
                        EmailAddress = model.EmailAddress,
                        PracticeManager = model.PracticeManager,
                        PurchaseManagerEmail = model.PurchaseManagerEmail
                    };

                    // Adding the hospital entity to the context
                    _context.DayHospital.Add(dayhospital);
                    _context.SaveChanges(); // Save changes
                }
                else
                {
                    // If a hospital with the same name or email already exists, return an error message
                    ViewBag.ErrorMessage = "Hospital with the same name or email address already exists.";
                    return View(model);
                }

                // Redirect to the success page after successful addition
                return RedirectToAction("AdminViewDayHospital", "Admin");
            }

            // Return the view with the model in case of an error
            return View(model);
        }
    }
}

