using Hospital.Data;
using Hospital.Models;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Hospital.Controllers
{
    public class OtherController(ApplicationDbContext dbContext) : Controller
    {
        ApplicationDbContext _context = dbContext;
        // GET: /Other/OtherLogin
        [HttpGet]
        public IActionResult OtherLogin()
        {
            // Return the login view to the user
            return View();
        }

        // POST: /Other/OtherLogin
        [HttpPost]
        public IActionResult OtherLogin(LoginViewModel model)
        {
            // Initialize the context to interact with the database
            if (ModelState.IsValid)
            {
                // Define the default password for validation
                const string defaultPassword = "Password123!";

                // Verify if the provided password matches the default password
                if (model.Password == defaultPassword)
                {
                    // Variables to determine the outcome and store user information
                    bool emailFound = false;
                    string redirectAction = string.Empty;
                    string redirectController = string.Empty;
                    string userName = string.Empty;
                    string userSurname = string.Empty;

                    // Determine the user role and perform the corresponding check
                    switch (model.Role)
                    {
                        case "Nurse":
                            // Attempt to find a nurse with the provided email address
                            var nurse = _context.Nurses.FirstOrDefault(n => n.EmailAddress == model.EmailAddress);
                            if (nurse != null)
                            {
                                // If found, set the emailFound flag and store user details
                                emailFound = true;
                                redirectAction = "Index";
                                redirectController = "Nurse";
                                // Update static class with user details
                                DisplayNameAndSurname.getUserName(nurse.Name);
                                DisplayNameAndSurname.getUserSurname(nurse.Surname);
                            }
                            break;

                        case "Surgeon":
                            // Attempt to find a surgeon with the provided email address
                            var surgeon = _context.Surgeons.SingleOrDefault(s => s.EmailAddress == model.EmailAddress);
                            if (surgeon != null)
                            {
                                // If found, set the emailFound flag and store user details
                                emailFound = true;
                                redirectAction = "Index";
                                redirectController = "Surgeon";
                                // Update static class with user details
                                DisplayNameAndSurname.getUserName(surgeon.Name);
                                DisplayNameAndSurname.getUserSurname(surgeon.Surname);
                            }
                            break;

                        case "Pharmacist":
                            // Attempt to find a pharmacist with the provided email address
                            var pharmacist = _context.Pharmacists.SingleOrDefault(p => p.EmailAddress == model.EmailAddress);
                            if (pharmacist != null)
                            {
                                // If found, set the emailFound flag and store user details
                                emailFound = true;
                                redirectAction = "Index";
                                redirectController = "Pharmacist";
                                // Update static class with user details
                                DisplayNameAndSurname.getUserName(pharmacist.Name);
                                DisplayNameAndSurname.getUserSurname(pharmacist.Surname);
                            }
                            break;

                        case "Admin":
                            // Attempt to find an admin with the provided email address
                            var admin = _context.AdminLogin.SingleOrDefault(a => a.EmailAddress == model.EmailAddress);
                            if (admin != null)
                            {
                                // If found, set the emailFound flag for Admin role
                                emailFound = true;
                                redirectAction = "Index";
                                redirectController = "Admin";
                            }
                            break;
                    }

                    // If an email match is found, redirect to the appropriate action and controller
                    if (emailFound)
                    {
                        TempData["SuccessMessage"] = "You are Logged in!" + @DisplayNameAndSurname.passUserName + " " + @DisplayNameAndSurname.passUserSurname;
                        return RedirectToAction(redirectAction, redirectController);
                    }
                    else
                    {
                        // Set an error message if the role and email do not match
                        ViewBag.ErrorMessage = "Invalid login attempt. The selected role does not match the email address.";
                    }
                }
                else
                {
                    // Set an error message if the password is incorrect
                    ViewBag.ErrorMessage = "Invalid login attempt. Incorrect password.";
                }
            }

            // Return the view with the model to display validation errors or login issues
            return View(model);
        }
    }
}
