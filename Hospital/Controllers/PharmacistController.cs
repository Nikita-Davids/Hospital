using Hospital.Data;
using Hospital.Models;
using Hospital.ModelViews;
using Hospital.ViewModels;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Hospital.Controllers
{
    public class PharmacistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PharmacistController(ApplicationDbContext context)
        {
            _context = context;
        }

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
                    // Populate the view model with details from each prescription
                    PatientIDNumber = p.PatientIdnumber,
                    PatientName = p.PatientName,
                    PatientSurname = p.PatientSurname,
                    PrescriptionDate = p.PrescriptionDate,

                    // Retrieve surgeon's name and surname using the surgeon's ID
                    SurgeonName = _context.Surgeons
                        .Where(s => s.SurgeonId == p.SurgeonId)
                        .Select(s => s.Name)
                        .FirstOrDefault(),
                    SurgeonSurname = _context.Surgeons
                        .Where(s => s.SurgeonId == p.SurgeonId)
                        .Select(s => s.Surname)
                        .FirstOrDefault(),

                    // Map the Urgent field from the prescription
                    Urgent = p.Urgent
                })
                // Order prescriptions by date in descending order
                .OrderByDescending(p => p.PrescriptionDate)
                // Convert the query result to a list
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

                    // Re-populate distinct ingredients dropdown
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

            // Re-populate distinct ingredients dropdown
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
            medication.MedicationActiveIngredients = CombinedIngredientsHidden;

            // Save changes to the database
            _context.Update(medication);
            _context.SaveChanges();

            return RedirectToAction("Index");
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

        // POST: Medications/Delete/5
        [HttpPost, ActionName("DeleteMedication")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Fetch the medication record based on the provided ID
            var medication = await _context.Medication.FindAsync(id);

            // Check if the medication was not found
            if (medication == null)
            {
                // Return a 404 Not Found response if no medication matches the ID
                return NotFound();
            }

            // Mark the medication as deleted by setting the IsDeleted flag
            medication.IsDeleted = "Deleted";

            // Update the medication record in the database
            _context.Update(medication);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Redirect to the action that displays the list of medications
            return RedirectToAction(nameof(ViewAddMedication));
        }

        // Method to check if medication exists
        private bool MedicationExists(int id)
        {
            return _context.Medication.Any(e => e.MedicationId == id);
        }


        // GET: /Pharmacist/ViewAddMedication
        public IActionResult ViewAddMedication()
        {
            var medications = _context.Medication.ToList(); // Get all medications
            return View(medications);
        }


        // Restock Controller Actions

        // GET: Retrieves dosage forms for a specified medication
        [HttpGet]
        public IActionResult GetDosageForms(string medicationName)
        {
            // Retrieve distinct dosage forms for the selected medication name
            var dosageForms = _context.Medication
                .Where(m => m.MedicationName == medicationName && m.IsDeleted != "Deleted") // Filter by medication name and not deleted
                .OrderBy(m => m.DosageForm) // Sort by DosageForm A-Z
                .Select(m => m.DosageForm) // Select dosage form
                .Distinct() // Get distinct dosage forms
                .ToList(); // Convert to list

            // Return the list of dosage forms as JSON
            return Json(dosageForms);
        }

        // GET: Display the restock form
        public IActionResult Restock()
        {
            // Retrieve distinct medication names, excluding those marked as deleted
            var medications = _context.Medication
                .Where(m => m.IsDeleted != "Deleted")
                .OrderBy(m => m.MedicationName) // Sort by MedicationName A-Z
                .Select(m => m.MedicationName) // Select medication names
                .Distinct() // Get distinct names
                .ToList(); // Convert to list

            // Pass the list of medication names to the view using ViewBag
            ViewBag.Medications = new SelectList(medications);

            // Return the view for restocking
            return View();
        }
        [HttpPost]
        public IActionResult Restock(Restock r)
        {
            if (!ModelState.IsValid)
            {
                // Re-populate the medication names for the view if validation fails
                var medications = _context.Medication
                    .Where(m => m.IsDeleted != "Deleted")
                    .OrderBy(m => m.MedicationName)
                    .Select(m => m.MedicationName)
                    .Distinct()
                    .ToList();

                ViewBag.Medications = new SelectList(medications);
                return View(r);
            }

            if (r.QuantityReceived <= 0)
            {
                ViewBag.Error = "Quantity must be greater than 0";
                return View(r);
            }

            try
            {
                var medication = _context.Medication
                    .FirstOrDefault(m => m.MedicationName == r.MedicationName && m.DosageForm == r.DosageForm);

                if (medication != null)
                {
                    r.MedicationId = medication.MedicationId;

                    _context.Restock.Add(r);

                    var stockEntry = _context.Stock
                        .FirstOrDefault(s => s.MedicationId == medication.MedicationId);

                    if (stockEntry != null)
                    {
                        stockEntry.StockOnHand += r.QuantityReceived;
                    }
                    else
                    {
                        _context.Stock.Add(new Stock
                        {
                            MedicationId = medication.MedicationId,
                            StockOnHand = r.QuantityReceived
                        });
                    }

                    _context.SaveChanges();
                    return RedirectToAction("ViewRestock", "Pharmacist");
                }
                else
                {
                    ViewBag.Error = "Invalid medication or dosage form selected";
                    return View(r);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred while restocking medication: " + ex.Message;
                return View(r);
            }
        }



        // GET action method for editing restock entries
        public IActionResult EditRestock(int id)
        {
            // Check if the provided id is valid
            if (id == 0)
            {
                // Return NotFound if the id is zero
                return NotFound();
            }

            // Retrieve the restock entry with the specified id
            Restock restock = _context.Restock
                .Where(e => e.RestockId == id) // Filter by RestockId
                .FirstOrDefault(); // Fetch the first matching record or null if not found

            // Check if the restock entry exists
            if (restock == null)
            {
                // Return NotFound if the restock entry does not exist
                return NotFound();
            }

            // Return the view for editing the restock entry, passing the restock object
            return View(restock);
        }

        // POST action method for updating restock entries
        [HttpPost]
        public IActionResult EditRestock([Bind("RestockId,MedicationName,DosageForm,QuantityReceived")] Restock r)
        {
            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                // Return the view with the current restock object if model state is invalid
                return View(r);
            }

            // Retrieve the existing restock entry with the specified id
            Restock existingRestock = _context.Restock
                .Where(e => e.RestockId == r.RestockId) // Filter by RestockId
                .FirstOrDefault(); // Fetch the first matching record or null if not found

            // Check if the restock entry exists
            if (existingRestock != null)
            {
                // Update the properties of the existing restock entry with the new values
                existingRestock.MedicationName = r.MedicationName;
                existingRestock.DosageForm = r.DosageForm;
                existingRestock.QuantityReceived = r.QuantityReceived;
                existingRestock.RestockDate = DateTime.Now; // Set the current date and time

                // Save changes to the database
                _context.SaveChanges();
            }

            // Redirect to the "ViewRestock" action in the "Pharmacist" controller after successful update
            return RedirectToAction("ViewRestock", "Pharmacist");
        }

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
            var restocks = query.ToList();

            // Pass the searchQuery to the view using ViewData for display or future use
            ViewData["SearchQuery"] = searchQuery;

            // Return the view, passing the filtered list of restocks
            return View(restocks);
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
 


    }
}
