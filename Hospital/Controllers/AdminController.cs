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
        public IActionResult AdminAddActiveIngredients()
        {
            var model = new ActiveIngredient();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminAddActiveIngredients(ActiveIngredient model)
        {
            if (ModelState.IsValid)
            {
                // Ensure that Strength is in the format "Amount Unit"
                string strength = model.Strength;

                // Check if an active ingredient with the same name and strength already exists
                var existingActiveIngredient = _context.ActiveIngredient
                    .FirstOrDefault(ai => ai.IngredientName.Trim().ToLower() == model.IngredientName.Trim().ToLower() &&
                                          ai.Strength.Trim().ToLower() == strength.Trim().ToLower());

                if (existingActiveIngredient == null)
                {
                    // Create a new ActiveIngredient entity
                    var activeIngredient = new ActiveIngredient
                    {
                        IngredientName = model.IngredientName,
                        Strength = strength
                    };

                    // Add the ActiveIngredient entity to the context
                    _context.ActiveIngredient.Add(activeIngredient);
                    _context.SaveChanges(); // Save changes

                    // Redirect to the success page after successful addition
                    return RedirectToAction("AdminViewActiveIngredients");
                }
                else
                {
                    // If the active ingredient with the same name and strength already exists, return an error message
                    ModelState.AddModelError("", "An active ingredient with the same name and strength already exists.");
                }
            }

            // Return the view with the model in case of an error
            return View(model);
        }

        // GET: ActiveIngredients/Edit/5
        public async Task<IActionResult> AdminEditActiveIngredients(int id)
        {
            // Retrieve the active ingredient record from the database using the provided ID
            var activeIngredient = await _context.ActiveIngredient.FindAsync(id);

            // Check if the active ingredient exists. If not, return a 404 Not Found response
            if (activeIngredient == null)
            {
                return NotFound();
            }

            // Set unit options in ViewBag as a SelectList
            ViewBag.Units = new SelectList(new[]
            {
            new { Value = "mg", Text = "Milligrams (mg)" },
            new { Value = "g", Text = "Grams (g)" }
        }, "Value", "Text");

            // Return the view with the active ingredient data to be edited
            return View(activeIngredient);
        }

        // POST: ActiveIngredients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminEditActiveIngredients(int id, [Bind("IngredientId,IngredientName,Strength")] ActiveIngredient activeIngredient)
        {
            // Check if the ID in the URL matches the ID of the active ingredient being edited
            if (id != activeIngredient.IngredientId)
            {
                return NotFound();
            }

            // Check if the new ingredient name and strength combination already exists in the database, excluding the current entry
            if (_context.ActiveIngredient.Any(ai => ai.IngredientName.Trim().ToLower() == activeIngredient.IngredientName.Trim().ToLower() &&
                                                    ai.Strength.Trim().ToLower() == activeIngredient.Strength.Trim().ToLower() &&
                                                    ai.IngredientId != id))
            {
                // Add a model error if the name and strength combination already exists and return the view with the error
                ModelState.AddModelError("IngredientName", "An active ingredient with the same name and strength already exists.");
                return View(activeIngredient);
            }

            // If the model state is valid, update the active ingredient in the database
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the active ingredient record in the database
                    _context.Update(activeIngredient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency issues if the record has been modified by another user
                    if (!ActiveIngredientExists(activeIngredient.IngredientId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the Index action if the update was successful
                return RedirectToAction(nameof(AdminViewActiveIngredients));
            }

            // Return the view with validation errors if the model state is not valid
            return View(activeIngredient);
        }

        // Method to check if an active ingredient with the same ID exists
        private bool ActiveIngredientExists(int id)
        {
            // Check if there is any active ingredient in the database with the specified ID
            return _context.ActiveIngredient.Any(e => e.IngredientId == id);
        }

        // Action method for displaying the list of added active ingredients
        public IActionResult AdminViewActiveIngredients()
        {
            // Retrieve all active ingredients records from the database and store them in a list
            var ingredients = _context.ActiveIngredient.ToList();

            // Pass the list of active ingredients to the view for display
            return View(ingredients);
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

        // GET: Hospitals/Edit/5
        public async Task<IActionResult> EditDayHospital(int id)
        {
            // Retrieve the hospital record from the database using the provided ID
            var hospital = await _context.DayHospital.FindAsync(id);

            // Check if the hospital exists. If not, return a 404 Not Found response.
            if (hospital == null)
            {
                return NotFound();
            }

            // Return the view with the hospital data to be edited
            return View(hospital);
        }

        // POST: Hospitals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDayHospital(int id, [Bind("HospitalId,HospitalName,Address,City,Province,PostalCode,ContactNumber,EmailAddress,PracticeManager,PurchaseManagerEmail")] DayHospital hospital)
        {
            // Check if the ID in the URL matches the ID of the hospital being edited
            if (id != hospital.HospitalId)
            {
                return NotFound();
            }

            // Check if the new hospital name already exists in the database, excluding the current entry
            if (_context.DayHospital.Any(h => h.HospitalName == hospital.HospitalName && h.HospitalId != id))
            {
                // Add a model error if the name already exists and return the view with the error
                ModelState.AddModelError("HospitalName", "The hospital name already exists.");
                return View(hospital);
            }

            // If the model state is valid, update the hospital in the database
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the hospital record in the database
                    _context.Update(hospital);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency issues if the record has been modified by another user
                    if (!HospitalExists(hospital.HospitalId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the Index action if the update was successful
                return RedirectToAction(nameof(AdminViewHospital));
            }

            // Return the view with validation errors if the model state is not valid
            return View(hospital);
        }

        // Helper method to check if a hospital exists in the database
        private bool HospitalExists(int id)
        {
            return _context.DayHospital.Any(e => e.HospitalId == id);
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
                return RedirectToAction("AdminViewHospital", "Admin");
            }

            // Return the view with the model in case of an error
            return View(model);
        }



        [HttpGet]
        public IActionResult AdminAddChronicCondition()
        {
            var model = new ChronicCondition();
            return View(model);
        }
        public IActionResult AdminAddChronicCondition(ChronicCondition model)
        {
            if (ModelState.IsValid)
            {
                // Check for existing ChronicCondition
                var existingCondition = _context.ChronicCondition
                    .FirstOrDefault(c => c.Icd10Code == model.Icd10Code || c.Diagnosis == model.Diagnosis);

                if (existingCondition == null)
                {
                    // Creating the chronic condition entity
                    var chronicCondition = new ChronicCondition
                    {
                        ChronicConditionId = model.ChronicConditionId,
                        Icd10Code = model.Icd10Code,
                        Diagnosis = model.Diagnosis
                    };

                    // Adding the ChronicCondition entity to the context
                    _context.ChronicCondition.Add(chronicCondition);
                    _context.SaveChanges(); // Save changes

                    // Redirect to the success page after successful addition
                    return RedirectToAction("ViewChronicConditions", "Admin");
                }
                else
                {
                    // If the chronic condition with the same ICD-10 code or diagnosis already exists, return an error message
                    ViewBag.ErrorMessage = "Chronic condition with the same ICD-10 code or diagnosis already exists.";
                    return View(model);
                }
            }

            // If the model state is not valid, return the view with the current model to display validation errors
            return View(model);
        }



        // GET: ChronicConditions/Edit/5
        public async Task<IActionResult> EditAdminChronicMedication(int id)
        {
            // Retrieve the chronic condition record from the database using the provided ID
            var chronicCondition = await _context.ChronicCondition.FindAsync(id);

            // Check if the chronic condition exists. If not, return a 404 Not Found response.
            if (chronicCondition == null)
            {
                return NotFound();
            }

            // Return the view with the chronic condition data to be edited
            return View(chronicCondition);
        }

        // POST: ChronicConditions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdminChronicMedication(int id, [Bind("ChronicConditionId,Icd10Code,Diagnosis")] ChronicCondition chronicCondition)
        {
            // Check if the ID in the URL matches the ID of the chronic condition being edited
            if (id != chronicCondition.ChronicConditionId)
            {
                return NotFound();
            }

            // Check if the new ICD-10 code already exists in the database, excluding the current entry
            if (_context.ChronicCondition.Any(c => c.Icd10Code == chronicCondition.Icd10Code && c.ChronicConditionId != id))
            {
                // Add a model error if the ICD-10 code already exists and return the view with the error
                ModelState.AddModelError("Icd10Code", "The ICD-10 code already exists.");
                return View(chronicCondition);
            }

            // If the model state is valid, update the chronic condition in the database
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the chronic condition record in the database
                    _context.Update(chronicCondition);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency issues if the record has been modified by another user
                    if (!ChronicConditionExists(chronicCondition.ChronicConditionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the Index action if the update was successful
                return RedirectToAction(nameof(ViewChronicConditions));
            }

            // Return the view with validation errors if the model state is not valid
            return View(chronicCondition);
        }

        // Helper method to check if a chronic condition exists in the database
        private bool ChronicConditionExists(int id)
        {
            return _context.ChronicCondition.Any(e => e.ChronicConditionId == id);
        }
    

public IActionResult ViewChronicConditions()
        {
            return View(_context.ChronicCondition);
        }




        [HttpGet]
        public IActionResult AdminAddTheatres()
        {
            var model = new OperatingTheatre();
            return View(model);
        }

        // POST: /OperatingTheatre/AdminAddOperatingTheatre
        [HttpPost]
        [ValidateAntiForgeryToken]
     

        public IActionResult AdminAddTheatres(OperatingTheatre model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check for existing OperatingTheatre
                    var existingTheatre = _context.OperatingTheatre
                        .FirstOrDefault(t => t.OperatingTheatreName == model.OperatingTheatreName);

                    if (existingTheatre == null)
                    {
                        // Adding the OperatingTheatre entity to the context
                        _context.OperatingTheatre.Add(model);
                        _context.SaveChanges(); // Save changes

                        // Redirect to the list view after successful addition
                        return RedirectToAction("ViewOperatingTheatres");
                    }
                    else
                    {
                        // If the operating theatre with the same name already exists, return an error message
                        ViewBag.ErrorMessage = "Operating theatre with the same name already exists.";
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    // Log exception and display error message
                    // You can use a logging framework or just use Console.WriteLine for debugging
                    Console.WriteLine($"Error: {ex.Message}");
                    ViewBag.ErrorMessage = "An error occurred while adding the operating theatre.";
                    return View(model);
                }
            }

            // Return the view with the model if validation fails
            return View(model);
        }

        public IActionResult ViewOperatingTheatres()
        {
            return View(_context.OperatingTheatre);
        }


        // GET: OperatingTheatres/Edit/5
        public async Task<IActionResult> EditAdminTheatres(int id)
        {
            // Retrieve the operating theatre record from the database using the provided ID
            var operatingTheatre = await _context.OperatingTheatre.FindAsync(id);

            // Check if the operating theatre exists. If not, return a 404 Not Found response.
            if (operatingTheatre == null)
            {
                return NotFound();
            }

            // Return the view with the operating theatre data to be edited
            return View(operatingTheatre);
        }

        // POST: OperatingTheatres/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdminTheatres(int id, [Bind("OperatingTheatreId,OperatingTheatreName")] OperatingTheatre operatingTheatre)
        {
            // Check if the ID in the URL matches the ID of the operating theatre being edited
            if (id != operatingTheatre.OperatingTheatreId)
            {
                return NotFound();
            }

            // Check if the new operating theatre name already exists in the database, excluding the current entry
            if (_context.OperatingTheatre.Any(o => o.OperatingTheatreName == operatingTheatre.OperatingTheatreName && o.OperatingTheatreId != id))
            {
                // Add a model error if the name already exists and return the view with the error
                ModelState.AddModelError("OperatingTheatreName", "The operating theatre name already exists.");
                return View(operatingTheatre);
            }

            // If the model state is valid, update the operating theatre in the database
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the operating theatre record in the database
                    _context.Update(operatingTheatre);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency issues if the record has been modified by another user
                    if (!OperatingTheatreExists(operatingTheatre.OperatingTheatreId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the Index action if the update was successful
                return RedirectToAction(nameof(ViewOperatingTheatres));
            }

            // Return the view with validation errors if the model state is not valid
            return View(operatingTheatre);
        }

        // Helper method to check if an operating theatre exists in the database
        private bool OperatingTheatreExists(int id)
        {
            return _context.OperatingTheatre.Any(e => e.OperatingTheatreId == id);
        }


    }
}





