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
            ViewBag.Categories = _context.MedicineCategories.ToList();
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
                        model.Image = ms.ToArray(); 
                    }
                }
                _context.Medicines.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Medicine added successfully!";
                return RedirectToAction("SearchMedicines","Medicine"); 
        }

    }
}
