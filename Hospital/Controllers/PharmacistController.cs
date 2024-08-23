using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Hospital.Controllers
{
    public class PharmacistController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public PharmacistController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        // GET: ActiveIngredients/AddIngredient
        public IActionResult AddIngredient()
        {
            return View();
        }

        // POST: ActiveIngredients/AddIngredient
        // POST: ActiveIngredients/AddIngredient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIngredient([Bind("ActiveIngredientName")] ActiveIngredients activeIngredients)
        {
            if (ModelState.IsValid)
            {
                dbContext.Add(activeIngredients);
                await dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(activeIngredients);
        }
    }
}

