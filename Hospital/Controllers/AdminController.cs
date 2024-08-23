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
            ApplicationDbContext POE = new ApplicationDbContext();

            // Check if the model state is valid (i.e., all required fields are filled out correctly).
            if (ModelState.IsValid)
            {
                // Convert the input to lower case for case-insensitive comparison
                var email = model.EmailAddress.Trim().ToLower();
                var registrationNumber = model.HealthCouncilRegistrationNumber.Trim().ToLower();

                // Check for duplicate email and registration number
                bool emailExists = await POE.Nurses.AnyAsync(n => n.EmailAddress.Trim().ToLower() == email) ||
                                   await POE.Surgeons.AnyAsync(s => s.EmailAddress.Trim().ToLower() == email) ||
                                   await POE.Pharmacists.AnyAsync(p => p.EmailAddress.Trim().ToLower() == email);

                bool registrationNumberExists = await POE.Nurses.AnyAsync(n => n.HealthCouncilRegistrationNumber.Trim().ToLower() == registrationNumber) ||
                                                await POE.Surgeons.AnyAsync(s => s.HealthCouncilRegistrationNumber.Trim().ToLower() == registrationNumber) ||
                                                await POE.Pharmacists.AnyAsync(p => p.HealthCouncilRegistrationNumber.Trim().ToLower() == registrationNumber);

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
                        POE.Nurses.Add(nurse);
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
                        POE.Surgeons.Add(surgeon);
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
                        POE.Pharmacists.Add(pharmacist);
                        break;

                    default:
                        // If the selected role is not valid, add an error to the ModelState and return the view with the current model.
                        ModelState.AddModelError("", "Invalid role selected.");
                        return View(model);
                }

                // Save changes to the database asynchronously.
                await POE.SaveChangesAsync();

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

                using (var context = new ApplicationDbContext())
                {
                    // Check if an active ingredient with the same name and strength already exists
                    var existingActiveIngredient = context.ActiveIngredient
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
                        context.ActiveIngredient.Add(activeIngredient);
                        context.SaveChanges(); // Save changes

                        // Redirect to the success page after successful addition
                        return RedirectToAction("AdminViewActiveIngredients");
                    }
                    else
                    {
                        // If the active ingredient with the same name and strength already exists, return an error message
                        ModelState.AddModelError("", "An active ingredient with the same name and strength already exists.");
                    }
                }
            }

            // Return the view with the model in case of an error
            return View(model);
        }

        // GET: ActiveIngredients/Edit/5
        public async Task<IActionResult> AdminEditActiveIngredients(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                // Retrieve the active ingredient record from the database using the provided ID
                var activeIngredient = await context.ActiveIngredient.FindAsync(id);

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
        }


        // POST: ActiveIngredients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminEditActiveIngredients(int id, [Bind("IngredientId,IngredientName,Strength")] ActiveIngredient activeIngredient)
        {
            using (var context = new ApplicationDbContext())
            {
                // Check if the ID in the URL matches the ID of the active ingredient being edited
                if (id != activeIngredient.IngredientId)
                {
                    return NotFound();
                }

                // Check if the new ingredient name and strength combination already exists in the database, excluding the current entry
                if (context.ActiveIngredient.Any(ai => ai.IngredientName.Trim().ToLower() == activeIngredient.IngredientName.Trim().ToLower() &&
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
                        context.Update(activeIngredient);
                        await context.SaveChangesAsync();
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
        }


        //Method to check if an active ingredient with the same strength exist
        private bool ActiveIngredientExists(int id)
        {
            // Create a new instance of the database context
            using (var context = new ApplicationDbContext())
            {
                // Check if there is any active ingredient in the database with the specified ID
                return context.ActiveIngredient.Any(e => e.IngredientId == id);
            }
        }


        // Action method for displaying the list of added active ingredients
        public IActionResult AdminViewActiveIngredients()
        {
            // Initialize the database context to access the Active Ingredients table
            ApplicationDbContext POE = new ApplicationDbContext();

            // Retrieve all active ingredients records from the database and store them in a list
            List<ActiveIngredient> temp = POE.ActiveIngredient.ToList();

            // Pass the list of active ingredients to the view for display
            return View(temp);
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
            ApplicationDbContext POE = new ApplicationDbContext();

            if (ModelState.IsValid)
            {
                // Convert the input to lower case for case-insensitive comparison
                var dosageFormName = model.DosageFormName.Trim().ToLower();

                // Check if a dosage form with the same name already exists
                var existingDosageForm = POE.DosageForm
                    .FirstOrDefault(df => df.DosageFormName.Trim().ToLower() == dosageFormName);

                if (existingDosageForm == null)
                {
                    // Adding the dosage form entity to the context
                    POE.DosageForm.Add(model);
                    POE.SaveChanges(); // Save changes

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
            ApplicationDbContext POE = new ApplicationDbContext();
            // Retrieve the dosage form record from the database using the provided ID
            var dosageForm = await POE.DosageForm.FindAsync(id);

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
            ApplicationDbContext POE = new ApplicationDbContext();
            // Create a new instance of the context to interact with the database
            if (id != dosageForm.DosageFormID)
            {
                // Check if the ID in the URL matches the ID of the dosage form being edited
                return NotFound();
            }

            // Check if the new dosage form name already exists in the database, excluding the current entry
            if (POE.DosageForm.Any(df => df.DosageFormName == dosageForm.DosageFormName && df.DosageFormID != id))
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
                    POE.Update(dosageForm);
                    await POE.SaveChangesAsync();
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
            ApplicationDbContext POE = new ApplicationDbContext();
            return POE.DosageForm.Any(e => e.DosageFormID == id);
        }

        // Action method for displaying the list of added dosage forms
        public IActionResult AdminViewDosageForms()
        {
            // Initialize the database context to access the Dosage Forms table
            ApplicationDbContext POE = new ApplicationDbContext();

            // Retrieve all dosage forms records from the database and store them in a list
            List<DosageForm> temp = POE.DosageForm.ToList();

            // Pass the list of medications to the view for display
            return View(temp);
        }
    }
}
