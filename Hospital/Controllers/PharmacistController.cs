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

            // Retrieve all prescriptions where the dispense status is "Not Dispense"
            var prescriptions = _context.SurgeonPrescription
                .Where(p => p.Dispense == "Not Dispense") // Filter prescriptions based on the Dispense field
                .Select(p => new PrescriptionDisplayViewModel
                {
                    PrescriptionId = p.PrescriptionId, // Ensure this is included for uniqueness
                    PatientIDNumber = p.PatientIdnumber,
                    PatientName = p.PatientName,
                    PatientSurname = p.PatientSurname,
                    PrescriptionDate = p.PrescriptionDate,

                    SurgeonName = _context.Surgeons
                        .Where(s => s.SurgeonId == p.SurgeonId)
                        .Select(s => s.Name)
                        .FirstOrDefault(),
                    SurgeonSurname = _context.Surgeons
                        .Where(s => s.SurgeonId == p.SurgeonId)
                        .Select(s => s.Surname)
                        .FirstOrDefault(),

                    Urgent = p.Urgent
                })
                .OrderByDescending(p => p.PrescriptionDate)
                .ToList();

            // Pass the list of prescriptions to the view
            return View(prescriptions);
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

        // POST: Medications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMedication(Medication medication)
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

                // Set IsDeleted to "Active"
                medication.IsDeleted = "Active";

                // Add the medication to the database
                _context.Medication.Add(medication);
                _context.SaveChanges();

                return RedirectToAction("ViewAddMedication");
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

            return RedirectToAction("ViewAddMedication");
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
            List<Medication> medications = _context.Medication.ToList();

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
            try
            {
                // Build the order details for the email body
                string orderDetails = "<ul>";

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
                        orderDetails += $"<li>Medication: {medication.MedicationName} ({medication.DosageForm}), " +
                                        $"Quantity: {item.QuantityToOrder}, Date: {item.OrderStockDate:yyyy-MM-dd}</li>";
                    }
                }

                orderDetails += "</ul>";

                // Define email details
                var emailMessage = new MimeMessage
                {
                    From = { new MailboxAddress("Stock Management", "noreply@hospital.com") },
                    To = { new MailboxAddress("Admin", "robertobooysen11@gmail.com") }, // Recipients email
                    Subject = "Stock Order Update",
                    Body = new BodyBuilder
                    {
                        HtmlBody = $@"
            <h3>Stock Order Update</h3>
            <p>The stock order has been successfully recorded in the system with the following details:</p>
            {orderDetails}
            <p>Thank you for using the hospital's stock management system.</p>"
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

        // GET: PrescribedScript
        public IActionResult PrescribedScript(string patientId)
        {
            // Retrieve prescription details for the specified patient ID
            var prescriptionDetails = _context.SurgeonPrescription
                .Where(p => p.PatientIdnumber == patientId) // Filter prescriptions by the patient ID
                .Select(p => new PrescribedScriptViewModel
                {
                    // Populate the view model with patient and prescription details
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

                    // Retrieve details of medications prescribed to the patient
                    Medications = _context.SurgeonPrescription
                        .Where(sp => sp.PatientIdnumber == patientId) // Filter medications by the patient ID
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
        public async Task<IActionResult> Dispense(string id)
        {
            // Fetch all prescribed scripts for the given patient ID
            var prescribedScripts = await _context.SurgeonPrescription
                .Where(ps => ps.PatientIdnumber == id)
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

                    // Check and log the pharmacist name and surname
                    var pharmacistName = DisplayNameAndSurname.passUserName ?? "Unknown Name";
                    var pharmacistSurname = DisplayNameAndSurname.passUserSurname ?? "Unknown Surname";

                    Console.WriteLine($"Updating pharmacist name to: {pharmacistName}");
                    Console.WriteLine($"Updating pharmacist surname to: {pharmacistSurname}");

                    // Update name and surname
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
                // Log the error (consider using a logging framework for real applications)
                Console.WriteLine($"Error dispensing medication: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Rollback the transaction if an error occurred
                await transaction.RollbackAsync();

                // Return a 500 Internal Server Error response
                return StatusCode(500, "An error occurred while dispensing medications.");
            }
        }


        [HttpPost]
        public IActionResult Reject(int prescriptionId, int surgeonId, string rejectionReason)
        {
            try
            {
                // Validate form values
                if (prescriptionId <= 0 || surgeonId <= 0 || string.IsNullOrEmpty(rejectionReason))
                {
                    return Json(new { success = false, message = "Invalid data." });
                }

                // Check if the PrescriptionId and SurgeonId exist in the database
                var prescription = _context.SurgeonPrescription
                    .FirstOrDefault(sp => sp.PrescriptionId == prescriptionId && sp.SurgeonId == surgeonId);

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

                // Create a new RejectedPrescription entity
                var rejectedPrescription = new RejectedPrescription
                {
                    PrescriptionId = prescriptionId,
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

        public IActionResult DetailsRejectedScript(int prescriptionId)
        {
            // Query to join the SurgeonPrescriptions and RejectedPrescription tables
            // Filter by the provided prescriptionId
            var rejectedPrescription = (from sp in _context.SurgeonPrescription
                                        join rp in _context.RejectedPrescription
                                        on sp.PrescriptionId equals rp.PrescriptionId
                                        where sp.PrescriptionId == prescriptionId
                                        select new RejectedPrescriptionViewModel
                                        {
                                            PrescriptionId = sp.PrescriptionId,
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


        public IActionResult ViewRejectScript()
        {
            // Query to join the SurgeonPrescriptions and RejectedPrescription tables
            // Filter for prescriptions where the Dispense status is "Rejected"
            var rejectedPrescriptions = from sp in _context.SurgeonPrescription
                                        join rp in _context.RejectedPrescription
                                        on sp.PrescriptionId equals rp.PrescriptionId
                                        where sp.Dispense == "Rejected"
                                        select new RejectedPrescriptionViewModel
                                        {
                                            PrescriptionId = sp.PrescriptionId,
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
            return View(rejectedPrescriptions.ToList());
        }

        // GET: Restock page with dropdowns for medication names and dosage forms
        public IActionResult Restock()
        {
            // Get medication names for the dropdown
            var medications = _context.Medication
                .Where(m => m.IsDeleted != "Deleted")
                .OrderBy(m => m.MedicationName)
                .Select(m => new { m.MedicationId, m.MedicationName })
                .ToList();

            // Populate ViewBag with medication names
            ViewBag.Medications = new SelectList(medications, "MedicationId", "MedicationName");

            // Initialize ViewBag for dosage forms
            ViewBag.DosageForms = new SelectList(Enumerable.Empty<SelectListItem>());

            return View();
        }

        // POST: Handle form submission and update dosage forms based on selected medication
        [HttpPost]
        public IActionResult Restock(Restock restock)
        {
            try
            {
                // Check if the model state is valid
                if (!ModelState.IsValid)
                {
                    // Re-populate the dropdowns if validation fails
                    PopulateDropDowns();
                    return View(restock);
                }

                // Save the restock data to the database
                _context.Restock.Add(restock);
                _context.SaveChanges();

                return RedirectToAction("ViewRestock");
            }
            catch
            {
                ViewBag.Error = "An error occurred while processing your request.";
                PopulateDropDowns();
                return View(restock);
            }
        }

        // GET: Get dosage forms for a specified medication

        //[HttpGet]
        //public JsonResult GetDosageForms(int medicationId)
        //{
        //    var dosageForms = _context.DosageForm
        //        .Where(d => d.MedicationId == medicationId) // Adjust the filtering based on your schema
        //        .Select(d => new { d.Id, d.FormName }) // Return necessary fields
        //        .ToList();

        //    return Json(dosageForms);
        //}


        // Method to populate dropdowns for medication names and dosage forms
        private void PopulateDropDowns()
        {
            var medications = _context.Medication
                .Where(m => m.IsDeleted != "Deleted")
                .OrderBy(m => m.MedicationName)
                .Select(m => new { m.MedicationId, m.MedicationName })
                .ToList();
            ViewBag.Medications = new SelectList(medications, "MedicationId", "MedicationName");

            ViewBag.DosageForms = new SelectList(Enumerable.Empty<SelectListItem>());
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

        public IActionResult RestockTest()
        {
            // Retrieve medications and dosage forms from the database
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

            return View();
        }

        [HttpPost]
        public IActionResult RestockTest(Restock restock)
        {
            try
            {
                if (restock.MedicationName == null || restock.DosageForm == null || restock.QuantityReceived == 0 ||restock.RestockDate==null|| restock.MedicationId == 0)
                {
                    ViewBag.Error = "Please enter all fields";
                    return View();
                }
                else
                {
                    Restock r = new Restock()
                    {
                        MedicationName = restock.MedicationName,
                        DosageForm = restock.DosageForm,
                        QuantityReceived = restock.QuantityReceived,
                        RestockDate = restock.RestockDate,
                        MedicationId = restock.MedicationId
                    };
                    _context.Restock.Add(r);
                    // Find the stock entry for the medication
                    var stockEntry = _context.Stock
                        .FirstOrDefault(s => s.MedicationId == r.MedicationId);

                    if (stockEntry != null)
                    {
                        // Medication exists in Stock table, update stock quantity
                        stockEntry.StockOnHand += r.QuantityReceived;
                    }
                    else
                    {
                        // Medication does not exist in Stock table, add new entry
                        _context.Stock.Add(new Stock
                        {
                            MedicationId = r.MedicationId ?? 0,
                            StockOnHand = r.QuantityReceived
                        });
                    }
                    _context.SaveChanges();

                    return RedirectToAction("ViewRestock");
                }
            }
            catch
            {
                ViewBag.Error = "Restock already in the database";
                return View();
            }
        }

    }

}



    

















