using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Controllers
{
    public class NurseController(ApplicationDbContext dbContext) : Controller
    {
        ApplicationDbContext _context = dbContext;
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult NurseDischargePatients()
        {
            // Fetch the list of patients and create a concatenated display name
            var patients = _context.Patients
                .Select(p => new
                {
                    PatientIDNumber = p.PatientIDNumber,
                    FullName = p.PatientName + " " + p.PatientSurname
                })
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
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("NurseViewDischarges", "Nurse");
            }
            return View(model);
        }

        public IActionResult NurseViewDischarges()
        {
            // Fetch all DischargedPatient entries
            var dischargedPatients = _context.DischargedPatients.ToList();
            return View(dischargedPatients);
        }
        // GET: Nurse/AddPatient
        public IActionResult NurseAddPatients()
        {
            return View();
        }

        // POST: Nurse/AddPatient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddPatients(Patients model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("NurseViewPatients");
            }
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
        public IActionResult NurseAddPatientVital()
        {
            // Populate ViewBag.PatientId with a list of patients for the dropdown
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName");
            return View();
        }

        // POST: Nurse/AddPatientVital
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NurseAddPatientVital(PatientVital model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("NurseViewPatientVitals"); // Redirect to a list view or another appropriate action
            }

            // Repopulate the dropdown list if the model state is not valid
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientIDNumber", "PatientName", model.PatientId);
            return View(model);
        }
    }
}
