using HealthConnect.Data;
using HealthConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthConnect.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult AddMedicine()
        {
            ViewBag.Categories = _context.MedicineCategories.ToList(); // Load categories for dropdown
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddMedicine(Medicine model, IFormFile Image)
        {
            
                if (Image != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        await Image.CopyToAsync(ms);
                        model.Image = ms.ToArray(); // Convert image to byte array
                    }
                }

                _context.Medicines.Add(model); // Add medicine to the database
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Medicine added successfully!";
                return RedirectToAction("SearchMedicines","User"); // Redirect to the list page
            

            //ViewBag.Categories = _context.MedicineCategories.ToList();
            //return View(model); // Reload form with validation errors
        }

    }
}
