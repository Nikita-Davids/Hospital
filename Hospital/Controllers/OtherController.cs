using Hospital.Data;
using Hospital.Models;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Hospital.Controllers
{
    public class OtherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OtherController(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        // GET: /Other/OtherLogin
        [HttpGet]
        public IActionResult OtherLogin()
        {
            return View();
        }

        //// POST: /Other/OtherLogin
        //[HttpPost]
        //public IActionResult OtherLogin(LoginViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Define the correct password for validation (this should come from a user store in a real system)
        //        const string defaultPassword = "Password123!";

        //        if (model.Password == defaultPassword)
        //        {
        //            // Check if the email exists in the Nurses table
        //            var nurse = _context.Nurses.FirstOrDefault(n => n.EmailAddress == model.EmailAddress);
        //            if (nurse != null)
        //            {
        //                // Redirect to NurseAlerts if a Nurse is found
        //                TempData["SuccessMessage"] = $"Welcome Nurse {nurse.Name} {nurse.Surname}";
        //                return RedirectToAction("NurseAlerts", "Nurse");
        //            }

        //            // Check if the email exists in the Surgeons table
        //            var surgeon = _context.Surgeons.FirstOrDefault(s => s.EmailAddress == model.EmailAddress);
        //            if (surgeon != null)
        //            {
        //                // Redirect to the Surgeon Index page if a Surgeon is found
        //                TempData["SuccessMessage"] = $"Welcome Surgeon {surgeon.Name} {surgeon.Surname}";
        //                return RedirectToAction("Index", "Surgeon");
        //            }

        //            // Check if the email exists in the Pharmacists table
        //            var pharmacist = _context.Pharmacists.FirstOrDefault(p => p.EmailAddress == model.EmailAddress);
        //            if (pharmacist != null)
        //            {
        //                // Redirect to the Pharmacist Index page if a Pharmacist is found
        //                TempData["SuccessMessage"] = $"Welcome Pharmacist {pharmacist.Name} {pharmacist.Surname}";
        //                return RedirectToAction("Index", "Pharmacist");
        //            }

        //            // Check if the email exists in the Admin table
        //            var admin = _context.AdminLogin.FirstOrDefault(a => a.EmailAddress == model.EmailAddress);
        //            if (admin != null)
        //            {
        //                // Redirect to the Admin Index page if an Admin is found
        //                TempData["SuccessMessage"] = "Welcome Admin";
        //                return RedirectToAction("Index", "Admin");
        //            }

        //            // If email is not found in any table, display an error message
        //            ViewBag.ErrorMessage = "Invalid login attempt. Email address not found.";
        //            return View(model);
        //        }
        //        else
        //        {
        //            // Incorrect password
        //            ViewBag.ErrorMessage = "Invalid login attempt. Incorrect password.";
        //            return View(model);
        //        }
        //    }

        //    // If model validation fails, return to the view
        //    return View(model);
        //}
        //public IActionResult OtherLogin(LoginViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        const string defaultPassword = "Password123!";

        //        // Ensure password matches
        //        if (model.Password == defaultPassword)
        //        {
        //            // Log the login attempt for debugging purposes
        //            Console.WriteLine($"Login attempt by {model.EmailAddress}");

        //            // Check if the email exists in the Nurses table
        //            var nurse = _context.Nurses.FirstOrDefault(n => n.EmailAddress == model.EmailAddress);
        //            if (nurse != null)
        //            {
        //                TempData["SuccessMessage"] = $"Welcome Nurse : {nurse.Name} {nurse.Surname}";
        //                return RedirectToAction("NurseDispensedAlert", "Nurse");
        //            }


        //            // Check if the email exists in the Surgeons table
        //            var surgeon = _context.Surgeons.FirstOrDefault(s => s.EmailAddress == model.EmailAddress);
        //            if (surgeon != null)
        //            {
        //                TempData["SuccessMessage"] = $"Welcome Surgeon: {surgeon.Name} {surgeon.Surname}";
        //                return RedirectToAction("AddSurgeonPrescription", "Surgeon");
        //            }

        //            // Check if the email exists in the Pharmacists table
        //            var pharmacist = _context.Pharmacists.FirstOrDefault(p => p.EmailAddress == model.EmailAddress);
        //            if (pharmacist != null)
        //            {
        //                TempData["SuccessMessage"] = $"Welcome Pharmacist :{pharmacist.Name} {pharmacist.Surname}";
        //                return RedirectToAction("Index", "Pharmacist");
        //            }

        //            // Check if the email exists in the Admin table
        //            var admin = _context.AdminLogin.FirstOrDefault(a => a.EmailAddress == model.EmailAddress);
        //            if (admin != null)
        //            {
        //                TempData["SuccessMessage"] = "Welcome Admin";
        //                return RedirectToAction("Index", "Admin");
        //            }

        //            // Log error if no matching email is found
        //            Console.WriteLine($"Login failed: Email {model.EmailAddress} not found in any role");
        //            ViewBag.ErrorMessage = "Invalid login attempt. Email address not found.";
        //            return View(model);
        //        }
        //        else
        //        {
        //            // Log incorrect password attempt
        //            Console.WriteLine($"Login failed: Incorrect password for {model.EmailAddress}");
        //            ViewBag.ErrorMessage = "Invalid login attempt. Incorrect password.";
        //            return View(model);
        //        }
        //    }

        //    // Log if ModelState is invalid
        //    Console.WriteLine("Login failed: ModelState is invalid");
        //    return View(model);
        //}

        [HttpPost]
        public IActionResult OtherLogin(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                const string defaultPassword = "Password123!";

                if (model.Password == defaultPassword)
                {
                    bool emailFound = false;
                    string redirectAction = string.Empty;
                    string redirectController = string.Empty;

                    // Try to find the user as a Nurse
                    var nurse = _context.Nurses.SingleOrDefault(n => n.EmailAddress == model.EmailAddress);
                    if (nurse != null)
                    {
                        emailFound = true;
                        redirectAction = "Index";
                        redirectController = "Nurse";
                        DisplayNameAndSurname.getUserName(nurse.Name);
                        DisplayNameAndSurname.getUserSurname(nurse.Surname);
                    }
                    else
                    {
                        // Try to find the user as a Surgeon
                        var surgeon = _context.Surgeons.SingleOrDefault(s => s.EmailAddress == model.EmailAddress);
                        if (surgeon != null)
                        {
                            emailFound = true;
                            redirectAction = "Index";
                            redirectController = "Surgeon";
                            DisplayNameAndSurname.getUserName(surgeon.Name);
                            DisplayNameAndSurname.getUserSurname(surgeon.Surname);
                        }
                        else
                        {
                            // Try to find the user as a Pharmacist
                            var pharmacist = _context.Pharmacists.SingleOrDefault(p => p.EmailAddress == model.EmailAddress);
                            if (pharmacist != null)
                            {
                                emailFound = true;
                                redirectAction = "Index";
                                redirectController = "Pharmacist";
                                DisplayNameAndSurname.getUserName(pharmacist.Name);
                                DisplayNameAndSurname.getUserSurname(pharmacist.Surname);
                            }
                            else
                            {
                                // Try to find the user as an Admin
                                var admin = _context.AdminLogin.SingleOrDefault(a => a.EmailAddress == model.EmailAddress);
                                if (admin != null)
                                {
                                    emailFound = true;
                                    redirectAction = "Index";
                                    redirectController = "Admin";
                                }
                            }
                        }
                    }

                    if (emailFound)
                    {
                        TempData["SuccessMessage"] = "You are logged in!" + @DisplayNameAndSurname.passUserName + " " + @DisplayNameAndSurname.passUserSurname;
                        return RedirectToAction(redirectAction, redirectController);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Invalid login attempt. Email not found.";
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid login attempt. Incorrect password.";
                }
            }
            return View(model);
        }


    }
}
