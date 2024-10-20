using Hospital.Data;
using Hospital.Models;
using Hospital.ModelViews;
using Hospital.ViewModels;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Hospital.ViewModels;
using System.Diagnostics;
using MailKit.Net.Smtp;

using System.Text;
using System.Net.Mail;
using System.Net;
using Hospital.Migrations;
using System.Drawing.Printing;
using System.Drawing;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp;

namespace Hospital.Controllers
{
    public class PharmacistController(ApplicationDbContext dbContext) : Controller
    {
        ApplicationDbContext _context = dbContext;



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        public IActionResult Index()
        {
            // Retrieve the user's full name from TempData
            ViewBag.UserName = TempData["UserName"];

            // Use _context to access the database
            var groupedPrescriptions = _context.SurgeonPrescription
                .Where(p => p.Dispense == "Not Dispense")
                .GroupBy(p => p.PrescribedID)
                .Select(g => new PrescriptionDisplayViewModel
                {
                    PrescribedID = g.Key,
                    PatientIDNumber = g.FirstOrDefault().PatientIdnumber,
                    PatientName = g.FirstOrDefault().PatientName,
                    PatientSurname = g.FirstOrDefault().PatientSurname,
                    PrescriptionDate = g.FirstOrDefault().PrescriptionDate,

                    // Retrieve surgeon's name and surname using the surgeon's ID
                    SurgeonName = _context.Surgeons
                        .Where(s => s.SurgeonId == g.FirstOrDefault().SurgeonId)
                        .Select(s => s.Name)
                        .FirstOrDefault(),
                    SurgeonSurname = _context.Surgeons
                        .Where(s => s.SurgeonId == g.FirstOrDefault().SurgeonId)
                        .Select(s => s.Surname)
                        .FirstOrDefault(),

                    // Map the Urgent field from the prescription
                    Urgent = g.FirstOrDefault().Urgent,

                    // Count the number of medications
                    MedicationCount = g.Count()
                })
                .OrderByDescending(p => p.PrescriptionDate)
                .ToList();

            // Pass the list of grouped prescriptions to the view
            return View(groupedPrescriptions);
        }

        public IActionResult ViewInfo()
        {
            return View();
        }

        // GET: Medications/Create
        public IActionResult AddMedication()
        {
            // Populate the dropdown list with dosage forms sorted alphabetically (A-Z)
            ViewBag.DosageForms = _context.DosageForm
                .OrderBy(a => a.DosageFormName) // Sort by DosageFormName A-Z
                .Select(a => new SelectListItem
                {
                    Value = a.DosageFormName,
                    Text = a.DosageFormName
                }).ToList();

            // Populate the dropdown list with distinct active ingredient names
            ViewBag.Ingredients = _context.ActiveIngredient
                .OrderBy(m => m.IngredientName) // Sort by IngredientName A-Z
                .Select(a => a.IngredientName)
                .Distinct()
                .Select(name => new SelectListItem
                {
                    Value = name,
                    Text = name
                }).ToList();

            return View();
        }

        // GET: Medications/GetStrengths
        public JsonResult GetStrengths(string ingredientName)
        {
            var strengths = _context.ActiveIngredient
                .Where(a => a.IngredientName == ingredientName)
                .Select(a => a.Strength)
                .Distinct()
                .ToList();

            return Json(strengths);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMedication(Medication medication, List<string> selectedIngredients, List<string> selectedStrengths)
        {
            // Debugging: Confirm received data
            ViewBag.DebugMedication = medication;
            ViewBag.DebugCombinedIngredients = medication.MedicationActiveIngredients;

            if (ModelState.IsValid)
            {
                // Normalize the input to lowercase for comparison
                string medicationNameLower = medication.MedicationName.Trim().ToLower();
                string dosageFormLower = medication.DosageForm?.Trim().ToLower();

                // Check if a medication with the same name and dosage form already exists (case-insensitive)
                bool medicationExists = _context.Medication
                    .Any(m => m.MedicationName.Trim().ToLower() == medicationNameLower
                              && m.DosageForm.Trim().ToLower() == dosageFormLower);

                if (medicationExists)
                {
                    // Add a model error for duplicate medication name
                    ModelState.AddModelError("MedicationName", "A medication with the same name already exists.");

                    // Re-populate ViewBag data for validation errors
                    ViewBag.DosageForms = _context.DosageForm
                        .OrderBy(a => a.DosageFormName) // Sort by DosageFormName A-Z
                        .Select(a => new SelectListItem
                        {
                            Value = a.DosageFormName,
                            Text = a.DosageFormName
                        }).ToList();

                    ViewBag.Ingredients = _context.ActiveIngredient
                        .OrderBy(m => m.IngredientName) // Sort by Ingredient Name A-Z
                        .Select(a => a.IngredientName)
                        .Distinct()
                        .Select(name => new SelectListItem
                        {
                            Value = name,
                            Text = name
                        }).ToList();

                    return View(medication);
                }

                // Check for duplicate active ingredients with the same strength
                var ingredientStrengthPairs = selectedIngredients.Zip(selectedStrengths, (ingredient, strength) => new { ingredient, strength })
                    .GroupBy(x => new { x.ingredient, x.strength }) // Group by ingredient and strength
                    .Where(g => g.Count() > 1) // Find groups with more than one occurrence
                    .Select(g => g.Key)
                    .ToList();

                if (ingredientStrengthPairs.Any())
                {
                    // Build error message for duplicate ingredient and strength pairs
                    string errorMessage = "You cannot select the same active ingredient with the same strength multiple times: ";
                    errorMessage += string.Join(", ", ingredientStrengthPairs.Select(pair => $"{pair.ingredient} (Strength: {pair.strength})"));

                    ModelState.AddModelError("ActiveIngredients", errorMessage);

                    // Re-populate ViewBag data for validation errors
                    ViewBag.DosageForms = _context.DosageForm
                        .OrderBy(a => a.DosageFormName) // Sort by DosageFormName A-Z
                        .Select(a => new SelectListItem
                        {
                            Value = a.DosageFormName,
                            Text = a.DosageFormName
                        }).ToList();

                    ViewBag.Ingredients = _context.ActiveIngredient
                        .OrderBy(m => m.IngredientName) // Sort by Ingredient Name A-Z
                        .Select(a => a.IngredientName)
                        .Distinct()
                        .Select(name => new SelectListItem
                        {
                            Value = name,
                            Text = name
                        }).ToList();

                    return View(medication);
                }

                // Set IsDeleted to "Active"
                medication.IsDeleted = "Active";

                // Add the medication to the database
                _context.Medication.Add(medication);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Medication added successfully.";
                return RedirectToAction("AddMedication");
            }

            // Re-populate ViewBag data for validation errors
            ViewBag.DosageForms = _context.DosageForm
                .OrderBy(a => a.DosageFormName) // Sort by DosageFormName A-Z
                .Select(a => new SelectListItem
                {
                    Value = a.DosageFormName,
                    Text = a.DosageFormName
                }).ToList();

            ViewBag.Ingredients = _context.ActiveIngredient
                .OrderBy(m => m.IngredientName) // Sort by IngredientName A-Z
                .Select(a => a.IngredientName)
                .Distinct()
                .Select(name => new SelectListItem
                {
                    Value = name,
                    Text = name
                }).ToList();

            return View(medication);
        }


        // GET: /Pharmacist/EditMedication/3
        [HttpGet("EditMedication/{id}")]
        public IActionResult EditMedication(int id)
        {
            // Load the medication
            var medication = _context.Medication
                .FirstOrDefault(m => m.MedicationId == id);

            if (medication == null)
            {
                return NotFound();
            }

            // Prepare dropdowns and data for the view
            var dosageForms = _context.DosageForm
                .OrderBy(m => m.DosageFormName) // Sort by DosageFormName A-Z
                .Select(df => new SelectListItem
                {
                    Value = df.DosageFormName,
                    Text = df.DosageFormName
                }).ToList();

            ViewBag.DosageForms = new SelectList(dosageForms, "Value", "Text", medication.DosageForm);

            var allIngredients = _context.ActiveIngredient
                .OrderBy(m => m.IngredientName) // Sort by IngredientName A-Z
                .Select(ai => new SelectListItem
                {
                    Value = ai.IngredientName,
                    Text = ai.IngredientName
                }).Distinct().ToList();

            ViewBag.Ingredients = new SelectList(allIngredients, "Value", "Text");

            // Prepare strengths based on ingredients
            var ingredientStrengths = _context.ActiveIngredient
                .GroupBy(ai => ai.IngredientName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ai => ai.Strength).Distinct().ToList()
                );

            ViewBag.IngredientStrengths = ingredientStrengths;

            // Pass the existing active ingredients as a comma-separated string to the view
            ViewBag.ExistingIngredients = medication.MedicationActiveIngredients;

            return View(medication);
        }

        // POST: /Pharmacist/EditMedication/3
        [HttpPost("EditMedication/{id}")]
        public IActionResult EditMedication(int id, Medication model, string CombinedIngredientsHidden)
        {
            // Load the medication to be updated
            var medication = _context.Medication
                .FirstOrDefault(m => m.MedicationId == id);

            if (medication == null)
            {
                return NotFound();
            }

            // Update medication details
            medication.MedicationName = model.MedicationName;
            medication.DosageForm = model.DosageForm;
            medication.Schedule = model.Schedule;
            medication.ReOrderLevel = model.ReOrderLevel;
            medication.IsDeleted = "Active";

            // Update MedicationActiveIngredients
            medication.MedicationActiveIngredients = !string.IsNullOrWhiteSpace(CombinedIngredientsHidden)
                ? CombinedIngredientsHidden
                : string.Empty;

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Medication successfully Edited.";
            return RedirectToAction("EditMedication");
        }

        // Method to display the medication deletion confirmation view
        public async Task<IActionResult> DeleteMedication(int id)
        {
            // Fetch the medication record based on the provided ID
            var medication = await _context.Medication
                .FirstOrDefaultAsync(m => m.MedicationId == id);

            // Check if the medication was not found
            if (medication == null)
            {
                // Return a 404 Not Found response if no medication matches the ID
                return NotFound();
            }

            // Return the view with the medication details to confirm deletion
            return View(medication);
        }

        // POST: Confirm and delete the medication
        [HttpPost, ActionName("DeleteMedication")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteMedication(int id)
        {
            // Fetch the medication record based on the provided ID
            var medication = await _context.Medication
                .FirstOrDefaultAsync(m => m.MedicationId == id);

            // Check if the medication was not found
            if (medication == null)
            {
                // Return a 404 Not Found response if no medication matches the ID
                return NotFound();
            }

            // Soft delete the medication by setting IsDeleted to "Deleted"
            medication.IsDeleted = "Deleted";

            // Update the medication record in the database
            await _context.SaveChangesAsync();

            // Redirect the user to the ViewAddMedication action after deletion
            return RedirectToAction("ViewAddMedication");
        }

        // GET: Pharmacist/ViewAddMedication
        public IActionResult ViewAddMedication()
        {
            // Retrieve all medication records from the database using _context
            // Sort the medications from the latest input to the oldest
            List<Medication> medications = _context.Medication
                .OrderByDescending(m => m.MedicationId) // Adjust this field if necessary
                .ToList();

            // Pass the list of medications to the view for display
            return View(medications);
        }


        // GET method to display the stock management view
        public async Task<IActionResult> StockManagement()
        {
            // Fetch all medications and their stock levels, including aggregate stock on hand
            var items = await _context.Medication
                .Select(m => new StockOrderViewModel
                {
                    MedicationId = m.MedicationId, // Medication identifier
                    MedicationName = m.MedicationName, // Name of the medication
                    DosageForm = m.DosageForm, // Dosage form (e.g., tablet, capsule)
                    StockOnHand = _context.Stock
                                         .Where(s => s.MedicationId == m.MedicationId) // Filter by medication
                                         .Sum(s => s.StockOnHand), // Sum of stock quantities
                    ReOrderLevel = m.ReOrderLevel // Reorder level for the medication
                })
                .ToListAsync(); // Execute the query asynchronously

            // Create and initialize the view model with the fetched items
            var viewModel = new StockOrderListViewModel
            {
                Items = items ?? new List<StockOrderViewModel>() // Ensure Items is initialized
            };

            // Pass the view model to the view
            return View(viewModel);
        }





        [HttpPost]
        public async Task<IActionResult> StockManagement(StockOrderListViewModel model)
        {
            // Check if the model state is valid and if there are any items in the model
            if (!ModelState.IsValid || model.Items == null)
            {
                return Json(new { success = false, message = "Invalid form data." });
            }

            // Filter selected items for ordering
            var selectedItems = model.Items.Where(item => item.IsSelected).ToList();

            // Check if any items were selected for ordering
            if (!selectedItems.Any())
            {
                return Json(new { success = false, message = "No items were selected for ordering." });
            }

            // Start a database transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Iterate through the selected items and create order stock entries
                foreach (var item in selectedItems)
                {
                    // Set the OrderStockDate to the current date if not provided
                    if (item.OrderStockDate == DateTime.MinValue)
                    {
                        item.OrderStockDate = DateTime.Now;
                    }

                    // Create a new OrderStock entry
                    var orderStock = new OrderStock
                    {
                        MedicationId = item.MedicationId,
                        QuantityOrdered = item.QuantityToOrder,
                        OrderStockDate = item.OrderStockDate
                    };

                    // Add the new OrderStock entry to the context
                    _context.OrderStock.Add(orderStock);
                }

                // Save changes to the database and commit the transaction
                int changes = await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return success message if changes were made
                if (changes > 0)
                {
                    // Send email notification with order details
                    await SendOrderStockSuccessEmail(selectedItems);

                    return Json(new { success = true, message = "Stock order updated and recorded successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "No changes were made to the database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions
                var errorData = selectedItems.Select(item => new
                {
                    MedicationId = item.MedicationId,
                    QuantityOrdered = item.QuantityToOrder,
                    OrderStockDate = item.OrderStockDate
                });

                // Log the error message and details
                var errorMessage = $"Database update error: {dbEx.Message}. Inner exception: {dbEx.InnerException?.Message}";
                Console.WriteLine(errorMessage);
                Console.WriteLine("Error data: " + string.Join(", ", errorData.Select(d => $"MedicationId={d.MedicationId}, QuantityOrdered={d.QuantityOrdered}, OrderStockDate={d.OrderStockDate:yyyy-MM-ddTHH:mm:ss.fff}")));

                // Rollback the transaction and return error message
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"A database error occurred. Details: {errorMessage}. Check the server logs for more information." });
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                var errorData = selectedItems.Select(item => new
                {
                    MedicationId = item.MedicationId,
                    QuantityOrdered = item.QuantityToOrder,
                    OrderStockDate = item.OrderStockDate
                });

                // Log the error message and details
                var errorMessage = $"General error occurred: {ex.Message}. Inner exception: {ex.InnerException?.Message}";
                Console.WriteLine(errorMessage);
                Console.WriteLine("Error data: " + string.Join(", ", errorData.Select(d => $"MedicationId={d.MedicationId}, QuantityOrdered={d.QuantityOrdered}, OrderStockDate={d.OrderStockDate:yyyy-MM-ddTHH:mm:ss.fff}")));

                // Rollback the transaction and return error message
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"An unexpected error occurred. Details: {errorMessage}" });
            }
        }

        private async Task SendOrderStockSuccessEmail(List<StockOrderViewModel> selectedItems)
        {
            // Check if there are any items to email about
            if (selectedItems == null || !selectedItems.Any())
            {
                Console.WriteLine("No items selected for email notification.");
                return;
            }

            try
            {
                // Build the order details for the email body
                StringBuilder orderDetailsBuilder = new StringBuilder("<ul>");

                foreach (var item in selectedItems)
                {
                    // Retrieve the medication name and dosage form based on the MedicationId
                    var medication = await _context.Medication
                        .Where(m => m.MedicationId == item.MedicationId)
                        .Select(m => new { m.MedicationName, m.DosageForm })
                        .FirstOrDefaultAsync();

                    // Add medication name, dosage form, quantity, and date to the email body
                    if (medication != null)
                    {
                        orderDetailsBuilder.Append($"<li>Medication: {medication.MedicationName} ({medication.DosageForm}), " +
                                                   $"Quantity: {item.QuantityToOrder}, Date: {item.OrderStockDate:yyyy-MM-dd}</li>");
                    }
                    else
                    {
                        Console.WriteLine($"Medication not found for MedicationId: {item.MedicationId}");
                    }
                }

                orderDetailsBuilder.Append("</ul>");

                // Define the email message
                var fromAddress = new MailAddress("kitadavids@gmail.com", "Stock Management");
                var toAddress = new MailAddress("kitadavids@gmail.com", "Admin");
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
                    Subject = "Northside Hospital-(Group 6-4th year Project)_ ",
                    Body = $@"
        <h3>Stock Order Update</h3>
        <p>The stock order has been successfully recorded in the system with the following details:</p>
        {orderDetailsBuilder}
        <p>Thank you for using the hospital's stock management system.</p>",
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




        // Action method for displaying ordered stock based on search query
        public IActionResult ViewOrderedStock(string searchQuery)
        {
            // Join OrderStocks with Medications to include medication details
            var query = _context.OrderStock
                .Join(_context.Medication,
                      orderStock => orderStock.MedicationId, // Foreign key from OrderStocks
                      medication => medication.MedicationId, // Primary key from Medications
                      (orderStock, medication) => new OrderStockViewModel
                      {
                          QuantityOrdered = orderStock.QuantityOrdered, // Quantity of medication ordered
                          OrderStockDate = orderStock.OrderStockDate, // Date when the stock was ordered
                          MedicationId = medication.MedicationId, // Medication identifier
                          MedicationName = medication.MedicationName // Medication name
                      });

            // Check if the searchQuery is a date or a medication name
            DateTime searchDate;
            if (DateTime.TryParse(searchQuery, out searchDate)) // If searchQuery can be parsed as a date
            {
                // Filter query by the searchDate
                query = query.Where(o => o.OrderStockDate.Date == searchDate.Date);
            }
            else if (!string.IsNullOrEmpty(searchQuery)) // If searchQuery is not empty
            {
                // Filter query by medication name containing the searchQuery
                query = query.Where(o => o.MedicationName.Contains(searchQuery));
            }

            // Execute the query and retrieve the results as a list
            var orderStockWithMedications = query.ToList();

            // Store the searchQuery in ViewData for use in the view
            ViewData["SearchQuery"] = searchQuery;

            // Return the view with the filtered list of ordered stock
            return View(orderStockWithMedications);
        }



        // GET: PharmacistPatientList
        public IActionResult PharmacistPatientList()
        {
            // Return the view with a list of Patients from the database
            return View(_context.Patients.ToList());
        }

        // Prescribed script based on PrescriptionId
        public IActionResult PrescribedScript(int PrescribedID)
        {
            // Retrieve prescription details for the specified PrescriptionId
            var prescriptionDetails = _context.SurgeonPrescription
                .Where(p => p.PrescribedID == PrescribedID) // Filter prescriptions by the PrescriptionId
                .Select(p => new PrescribedScriptViewModel
                {
                    // Populate the view model with patient and prescription details
                    PrescribedID = p.PrescribedID,
                    PatientIDNumber = p.PatientIdnumber,
                    PatientName = p.PatientName,
                    PatientSurname = p.PatientSurname,

                    // Retrieve the surgeon's name and surname using the surgeon's ID
                    SurgeonName = _context.Surgeons
                        .Where(s => s.SurgeonId == p.SurgeonId)
                        .Select(s => s.Name)
                        .FirstOrDefault(),
                    SurgeonSurname = _context.Surgeons
                        .Where(s => s.SurgeonId == p.SurgeonId)
                        .Select(s => s.Surname)
                        .FirstOrDefault(),

                    // Include the date of prescription
                    PrescriptionDate = p.PrescriptionDate,
                    PrescriptionId = p.PrescriptionId,
                    SurgeonId = p.SurgeonId,

                    // Retrieve details of medications prescribed for this prescription
                    Medications = _context.SurgeonPrescription
                        .Where(sp => sp.PrescribedID == PrescribedID) // Filter medications by the PrescriptionId
                        .Select(sp => new MedicationDetailViewModel
                        {
                            // Populate each medication detail
                            MedicationName = sp.MedicationName,
                            PrescriptionDosageForm = sp.PrescriptionDosageForm,
                            Instructions = sp.Instructions,
                            Quantity = sp.Quantity
                        })
                        .ToList() // Convert the result to a list
                })
                .FirstOrDefault(); // Retrieve the first (or default) result

            // Return a 404 Not Found response if no prescription details were found
            if (prescriptionDetails == null)
            {
                return NotFound();
            }

            // Pass the prescription details to the view
            return View(prescriptionDetails);
        }

        [HttpGet]
        public async Task<IActionResult> Dispense(int id)
        {
            // Fetch all prescribed scripts for the given patient ID using _context
            var prescribedScripts = await _context.SurgeonPrescription
                .Where(ps => ps.PrescribedID == id)
                .ToListAsync();

            // Check if any prescribed scripts were found
            if (prescribedScripts == null || !prescribedScripts.Any())
            {
                // Return a 404 Not Found response if no prescriptions are found
                return NotFound("Prescription not found.");
            }

            // Begin a database transaction to ensure all operations succeed together
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Process each prescribed script
                foreach (var script in prescribedScripts)
                {
                    // Fetch the current stock information for the medication
                    var stock = await _context.Stock
                        .FirstOrDefaultAsync(s => s.MedicationId == script.MedicationId);

                    // Throw an exception if the stock record is not found
                    if (stock == null)
                    {
                        throw new InvalidOperationException($"Stock record not found for Medication ID {script.MedicationId}.");
                    }

                    // Check if there is no stock available
                    if (stock.StockOnHand <= 0)
                    {
                        // Return a JSON response indicating no stock is available
                        return Json(new { success = false, message = $"No stock available for Medication ID {script.MedicationId}." });
                    }

                    // Check if there is enough stock to fulfill the prescription
                    if (stock.StockOnHand < script.Quantity)
                    {
                        // Return a JSON response indicating insufficient stock
                        return Json(new { success = false, message = $"Insufficient stock for Medication ID {script.MedicationId}. Available stock: {stock.StockOnHand}." });
                    }

                    // Subtract the prescribed quantity from the stock on hand
                    stock.StockOnHand -= script.Quantity;

                    // Update the Dispense field to "Dispense"
                    script.Dispense = "Dispense";

                    // Update pharmacist name and surname
                    var pharmacistName = DisplayNameAndSurname.passUserName ?? "Unknown Name";
                    var pharmacistSurname = DisplayNameAndSurname.passUserSurname ?? "Unknown Surname";
                    script.PharmacistName = pharmacistName;
                    script.PharmacistSurname = pharmacistSurname;

                    // Record the current time as the dispense date and time
                    script.DispenseDateTime = DateTime.Now;
                }

                // Save all changes to the database
                await _context.SaveChangesAsync();

                // Commit the transaction if all operations succeeded
                await transaction.CommitAsync();

                // Return a JSON response indicating success
                return Json(new { success = true, message = "Stock successfully updated and prescription dispensed." });
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error dispensing medication: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Rollback the transaction if an error occurred
                await transaction.RollbackAsync();

                // Return a 500 Internal Server Error response
                return StatusCode(500, "An error occurred while dispensing medications.");
            }
        }



        [HttpPost]
        public IActionResult Reject(int PrescribedID, int surgeonId, string rejectionReason)
        {
            try
            {
                // Validate form values
                if (PrescribedID <= 0 || surgeonId <= 0 || string.IsNullOrEmpty(rejectionReason))
                {
                    return Json(new { success = false, message = "Invalid data." });
                }

                // Check if the PrescribedID and SurgeonId exist in the database
                var prescription = _context.SurgeonPrescription
                    .FirstOrDefault(sp => sp.PrescribedID == PrescribedID && sp.SurgeonId == surgeonId);

                if (prescription == null)
                {
                    return Json(new { success = false, message = "Invalid prescription or surgeon ID." });
                }

                // Update the Dispense field to "Rejected"
                prescription.Dispense = "Rejected";

                // Update pharmacist information
                var pharmacistName = DisplayNameAndSurname.passUserName ?? "Unknown Name";
                var pharmacistSurname = DisplayNameAndSurname.passUserSurname ?? "Unknown Surname";

                prescription.PharmacistName = pharmacistName;
                prescription.PharmacistSurname = pharmacistSurname;

                prescription.DispenseDateTime = DateTime.Now;

                // Create a new RejectedPrescription entity
                var rejectedPrescription = new RejectedPrescription
                {
                    PrescribedID = PrescribedID,
                    SurgeonId = surgeonId,
                    RejectionReason = rejectionReason,
                    Status = "Pending",
                    PharmacistName = pharmacistName,
                    PharmacistSurname = pharmacistSurname
                };

                // Add the new entity to the context
                _context.RejectedPrescription.Add(rejectedPrescription);

                // Save changes to the database
                _context.SaveChanges();

                // Return a successful response with redirect URL
                return Json(new { success = true, message = "Prescription rejected successfully.", redirectUrl = Url.Action("ViewRejectScript", "Pharmacist") });
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred while rejecting the prescription: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner exception: {ex.InnerException.Message}";
                }
                return Json(new { success = false, message = errorMessage });
            }
        }


        public IActionResult ViewMedicationDispensed()
        {
            // Retrieve all surgeon prescriptions where Dispense is set to 'Dispense'
            var dispensedMedications = _context.SurgeonPrescription
                .Where(sp => sp.Dispense == "Dispense")
                .ToList();

            // Pass the list of dispensed medications to the view for display
            return View(dispensedMedications);
        }


        public IActionResult DetailsDispensedMedication(int id)
        {
            // Check if the provided 'id' parameter is zero (0)
            if (id <= 0)
            {
                return NotFound();
            }

            // Retrieve the SurgeonPrescription with the specified id
            var sp = _context.SurgeonPrescription
                .SingleOrDefault(e => e.PrescriptionId == id);

            // Check if the prescription was found
            if (sp == null)
            {
                return NotFound();
            }

            // Return the view with the retrieved SurgeonPrescription object
            return View(sp);
        }

        public IActionResult DetailsRejectedScript(int PrescribedID)
        {
            // Query to join the SurgeonPrescriptions and RejectedPrescription tables
            // Filter by the provided PrescribedID
            var rejectedPrescription = (from sp in _context.SurgeonPrescription
                                        join rp in _context.RejectedPrescription
                                        on sp.PrescribedID equals rp.PrescribedID
                                        where sp.PrescribedID == PrescribedID
                                        select new RejectedPrescriptionViewModel
                                        {
                                            PrescribedID = sp.PrescribedID,
                                            PatientIDNumber = sp.PatientIdnumber,
                                            PatientName = sp.PatientName,
                                            PatientSurname = sp.PatientSurname,
                                            MedicationName = sp.MedicationName,
                                            PrescriptionDosageForm = sp.PrescriptionDosageForm,
                                            Quantity = sp.Quantity,
                                            Instructions = sp.Instructions,
                                            Urgent = sp.Urgent,
                                            SurgeonID = sp.SurgeonId,
                                            PrescriptionDate = sp.PrescriptionDate,
                                            DispenseDateTime = sp.DispenseDateTime,
                                            PharmacistName = rp.PharmacistName,
                                            PharmacistSurname = rp.PharmacistSurname,
                                            RejectionReason = rp.RejectionReason,
                                            RejectionDate = rp.RejectionDate,
                                            Status = rp.Status
                                        }).FirstOrDefault();

            // Check if the query result is null, which means no matching record was found
            if (rejectedPrescription == null)
            {
                return NotFound(); // Return a 404 Not Found response if the prescription was not found
            }

            // Pass the rejectedPrescription model to the view for rendering
            return View(rejectedPrescription);
        }



        public IActionResult ViewRejectedScript()
        {
            // Query to join the SurgeonPrescriptions and RejectedPrescription tables
            // Filter for prescriptions where the Dispense status is "Rejected"
            var rejectedPrescriptions = from sp in _context.SurgeonPrescription
                                        join rp in _context.RejectedPrescription
                                        on sp.PrescribedID equals rp.PrescribedID
                                        where sp.Dispense == "Rejected"
                                        select new RejectedPrescriptionViewModel
                                        {
                                            PrescribedID = sp.PrescribedID,
                                            PatientIDNumber = sp.PatientIdnumber,
                                            PatientName = sp.PatientName,
                                            PatientSurname = sp.PatientSurname,
                                            MedicationName = sp.MedicationName,
                                            PrescriptionDosageForm = sp.PrescriptionDosageForm,
                                            Quantity = sp.Quantity,
                                            RejectionReason = rp.RejectionReason,
                                            RejectionDate = rp.RejectionDate,
                                            PharmacistName = rp.PharmacistName,
                                            PharmacistSurname = rp.PharmacistSurname,
                                            Status = rp.Status
                                        };

            // Convert the query result to a list and pass it to the view
            // This allows the view to render a list of rejected prescriptions
            return View(rejectedPrescriptions.ToList());
        }



        [HttpGet]
        public JsonResult GetMedicationId(string medicationName, string dosageForm)
        {
            var medicationId = _context.Medication
                .Where(m => m.MedicationName == medicationName && m.DosageForm == dosageForm && m.IsDeleted != "Deleted")
                .Select(m => m.MedicationId)
                .FirstOrDefault();

            return Json(medicationId);
        }

        [HttpGet]
        public IActionResult Restock()
        {
            var medications = _context.Medication
                .Where(m => m.IsDeleted != "Deleted")
                .OrderBy(m => m.MedicationName)
                .Select(m => new SelectListItem
                {
                    Value = m.MedicationName,
                    Text = m.MedicationName
                }).ToList();

            ViewBag.Medication = medications; // Correctly set ViewBag for medications

            // Fetch and group dosage forms by medication name
            var dosageForms = _context.Medication
                .Where(m => m.IsDeleted != "Deleted")
                .GroupBy(m => m.MedicationName)
                .ToDictionary(g => g.Key, g => g.Select(m => m.DosageForm).Distinct().ToList());

            // Pass dosage forms to the view
            ViewBag.DosageForms = dosageForms;

            return View();
        }





        [HttpPost]
        public IActionResult Restock(Restock restock)
        {
            try
            {
                if (string.IsNullOrEmpty(restock.MedicationName) || string.IsNullOrEmpty(restock.DosageForm) || restock.QuantityReceived == 0 || restock.RestockDate == null || restock.MedicationId == 0)
                {
                    ViewBag.Error = "Please enter all fields";
                    return View();
                }

                Restock newRestock = new Restock
                {
                    MedicationName = restock.MedicationName,
                    DosageForm = restock.DosageForm,
                    QuantityReceived = restock.QuantityReceived,
                    RestockDate = restock.RestockDate,
                    MedicationId = restock.MedicationId
                };

                _context.Restock.Add(newRestock);

                var stockEntry = _context.Stock
                    .FirstOrDefault(s => s.MedicationId == restock.MedicationId);

                if (stockEntry != null)
                {
                    stockEntry.StockOnHand += restock.QuantityReceived;
                }
                else
                {
                    _context.Stock.Add(new Stock
                    {
                        MedicationId = restock.MedicationId ?? 0,
                        StockOnHand = restock.QuantityReceived
                    });
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Restock of medication successfully added.";
                return RedirectToAction("Restock");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred: " + ex.Message;
                return View();
            }
        }


        // GET: Restock
        // Action method for displaying the list of restocks, with optional search functionality
        public IActionResult ViewRestock(string searchQuery)
        {
            // Initialize the queryable collection of restocks
            var query = _context.Restock.AsQueryable();

            // Attempt to parse the searchQuery as a date
            DateTime searchDate;
            if (DateTime.TryParse(searchQuery, out searchDate))
            {
                // Extract the start of the day from the searchDate
                var startDate = searchDate.Date; // Represents 00:00 of the specified date

                // Calculate the end of the day (start of the next day)
                var endDate = startDate.AddDays(1); // Represents 00:00 of the following day

                // Filter the restocks by RestockDate, ensuring only entries for the specified date are included
                query = query.Where(r => r.RestockDate >= startDate && r.RestockDate < endDate);
            }
            else if (!string.IsNullOrEmpty(searchQuery))
            {
                // Filter the restocks by MedicationName if searchQuery is not empty
                query = query.Where(r => r.MedicationName.Contains(searchQuery));
            }

            // Execute the query and retrieve the filtered list of restocks
            // Sort the restocks by RestockDate descending
            var restocks = query.OrderByDescending(r => r.RestockDate).ToList();

            // Pass the searchQuery to the view using ViewData for display or future use
            ViewData["SearchQuery"] = searchQuery;

            // Return the view, passing the filtered list of restocks
            return View(restocks);
        }

        [HttpGet]
        public IActionResult EditRestock(int id)
        {
            // Find the existing Restock by ID
            var restock = _context.Restock.Find(id);

            if (restock == null)
            {
                return NotFound();
            }

            // Retrieve medications and dosage forms
            var medications = _context.Medication
                .Where(m => m.IsDeleted != "Deleted")
                .OrderBy(m => m.MedicationName)
                .Select(m => new
                {
                    m.MedicationName,
                    m.DosageForm
                })
                .ToList();

            // Prepare data for ViewBag
            ViewBag.Medications = medications.Select(m => m.MedicationName).Distinct().ToList();
            ViewBag.DosageForms = medications
                .GroupBy(m => m.MedicationName)
                .ToDictionary(g => g.Key, g => g.Select(m => m.DosageForm).Distinct().ToList());

            return View(restock);
        }

        [HttpPost]
        public IActionResult EditRestock(Restock restock)
        {
            try
            {
                if (restock.MedicationName == null || restock.DosageForm == null || restock.QuantityReceived == 0 || restock.MedicationId == 0)
                {
                    ViewBag.Error = "Please enter all fields";
                    return View(restock);
                }
                else
                {
                    // Fetch the existing restock entry
                    var existingRestock = _context.Restock.Find(restock.RestockId);

                    if (existingRestock == null)
                    {
                        return NotFound();
                    }

                    // Fetch the current stock for this medication
                    var stock = _context.Stock.FirstOrDefault(s => s.MedicationId == restock.MedicationId);

                    if (stock == null)
                    {
                        // If no stock exists, create a new stock entry
                        stock = new Stock
                        {
                            MedicationId = restock.MedicationId.Value, // Cast it to int
                            StockOnHand = 0 // Initialize stock if it doesn't exist
                        };
                        _context.Stock.Add(stock);
                    }

                    // Calculate the difference in the QuantityReceived
                    int quantityDifference = restock.QuantityReceived - existingRestock.QuantityReceived;

                    // Update the stock based on the difference
                    stock.StockOnHand += quantityDifference;

                    // Update the existing restock entry
                    existingRestock.MedicationName = restock.MedicationName;
                    existingRestock.DosageForm = restock.DosageForm;
                    existingRestock.QuantityReceived = restock.QuantityReceived;
                    existingRestock.MedicationId = restock.MedicationId;

                    // Save the changes to the restock and stock
                    _context.SaveChanges();

                    // Set the success message in TempData
                    TempData["SuccessMessage"] = "Successfully edited the restock entry.";

                    return RedirectToAction("ViewRestock");
                }
            }
            catch
            {
                ViewBag.Error = "Error updating the restock entry";
                return View(restock);
            }
        }


        public async Task<IActionResult> PharmacistViewPatientDetails()
        {
            var patients = await _context.Patients.ToListAsync();
            return View(patients);
        }

        public async Task<IActionResult> PharmacistPatientOverview(string patientId)
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

            // Fetch allergies (empty list if not found) and sort alphabetically
            var allergies = await _context.PatientAllergies
                                           .Where(a => a.PatientId == patientId)
                                           .OrderBy(a => a.Allergy) // Sort alphabetically by allergy name
                                           .ToListAsync();

            // Fetch current medications (empty list if not found) and sort alphabetically
            var currentMedications = await _context.PatientCurrentMedication
                                                   .Where(m => m.PatientId == patientId)
                                                   .OrderBy(m => m.CurrentMedication) // Sort alphabetically by medication name
                                                   .ToListAsync();

            // Fetch medical conditions (empty list if not found) and sort alphabetically
            var medicalConditions = await _context.PatientMedicalCondition
                                                  .Where(c => c.PatientId == patientId)
                                                  .OrderBy(c => c.MedicalCondition) // Sort alphabetically by condition name
                                                  .ToListAsync();

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
                BMI = vitals?.BMI ?? null,
                Temperature = vitals?.Tempreture ?? null,
                BloodPressure = vitals?.BloodPressure ?? null,
                Pulse = vitals?.Pulse ?? null,
                Respiratory = vitals?.Respiratory ?? null,
                BloodOxygen = vitals?.BloodOxygen ?? null,
                BloodGlucoseLevel = vitals?.BloodGlucoseLevel ?? null,
                VitalTime = vitals?.VitalTime ?? null,
                Allergies = allergies, // Will be a sorted list
                CurrentMedications = currentMedications, // Sorted list
                MedicalConditions = medicalConditions // Sorted list
            };

            return View(model);
        }

        public async Task<IActionResult> ViewSpecificPatientDetails(string patientId)
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

            // Fetch allergies (empty list if not found) and sort alphabetically
            var allergies = await _context.PatientAllergies
                                           .Where(a => a.PatientId == patientId)
                                           .OrderBy(a => a.Allergy) // Sort alphabetically by allergy name
                                           .ToListAsync();

            // Fetch current medications (empty list if not found) and sort alphabetically
            var currentMedications = await _context.PatientCurrentMedication
                                                   .Where(m => m.PatientId == patientId)
                                                   .OrderBy(m => m.CurrentMedication) // Sort alphabetically by medication name
                                                   .ToListAsync();

            // Fetch medical conditions (empty list if not found) and sort alphabetically
            var medicalConditions = await _context.PatientMedicalCondition
                                                  .Where(c => c.PatientId == patientId)
                                                  .OrderBy(c => c.MedicalCondition) // Sort alphabetically by condition name
                                                  .ToListAsync();

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
                BMI = vitals?.BMI ?? null,
                Temperature = vitals?.Tempreture ?? null,
                BloodPressure = vitals?.BloodPressure ?? null,
                Pulse = vitals?.Pulse ?? null,
                Respiratory = vitals?.Respiratory ?? null,
                BloodOxygen = vitals?.BloodOxygen ?? null,
                BloodGlucoseLevel = vitals?.BloodGlucoseLevel ?? null,
                VitalTime = vitals?.VitalTime ?? null,
                Allergies = allergies, // Will be a sorted list
                CurrentMedications = currentMedications, // Sorted list
                MedicalConditions = medicalConditions // Sorted list
            };

            return View(model);
        }

        public async Task<IActionResult> ViewRejectScript()
        {
            // Query to join the SurgeonPrescriptions and RejectedPrescription tables
            // Filter for prescriptions where the Dispense status is "Rejected"
            var rejectedPrescriptions = await (from sp in _context.SurgeonPrescription
                                               join rp in _context.RejectedPrescription // Make sure this is the correct DbSet name
                                               on sp.PrescribedID equals rp.PrescribedID
                                               where sp.Dispense == "Rejected"
                                               select new RejectedPrescriptionViewModel
                                               {
                                                   PrescribedID = sp.PrescribedID,
                                                   PatientIDNumber = sp.PatientIdnumber,
                                                   PatientName = sp.PatientName,
                                                   PatientSurname = sp.PatientSurname,
                                                   MedicationName = sp.MedicationName,
                                                   PrescriptionDosageForm = sp.PrescriptionDosageForm,
                                                   Quantity = sp.Quantity,
                                                   RejectionReason = rp.RejectionReason,
                                                   RejectionDate = rp.RejectionDate,
                                                   PharmacistName = rp.PharmacistName,
                                                   PharmacistSurname = rp.PharmacistSurname,
                                                   Status = rp.Status
                                               }).ToListAsync(); // Use ToListAsync for async operation

            // Pass the list to the view for rendering
            return View(rejectedPrescriptions);
        }

        //Displaying pdf info
        [HttpGet]
        public IActionResult FilterPrescription(DateTime? startDate, DateTime? endDate)
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

































