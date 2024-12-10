namespace HealthConnect.Controllers
{
    public class MedicineController : Controller
    {
        private readonly IMedicineService _medicineService;

        public MedicineController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        [HttpGet]
        public IActionResult SearchSuggestions(string query)
        {
            var suggestions = _medicineService.GetSuggestions(query);
            return Json(suggestions.Take(4));
        }

        [HttpGet]
        public IActionResult SearchMedicines()
        {
            return View();
        }

        public IActionResult DisplayMedicines(int categoryId)
        {
            var model = _medicineService.GetMedicinesByCategory(categoryId);
            ViewBag.CategoryId = categoryId;
            return View(model);
        }

        [HttpGet]
        public IActionResult MedicineInfo(int id)
        {
            var medicine = _medicineService.GetMedicineById(id);

            if (medicine == null)
                return NotFound();

            return View(medicine);
        }

        [HttpGet]
        public IActionResult MedicineInfoByName(string medicineName)
        {
            var medicine = _medicineService.GetMedicineByName(medicineName);

            if (medicine == null)
                return NotFound("Medicine not found.");

            return View("MedicineInfo", medicine);
        }

        [HttpGet]
        public IActionResult SearchByProduct(string medicineName, int categoryId)
        {
            var medicines = _medicineService.GetMedicineByNameAndCategory(medicineName, categoryId);
            ViewBag.CategoryId = categoryId;
            return View("DisplayMedicines", medicines);
        }

        [HttpGet("GetImage/{id}")]
        public IActionResult GetImage(int id)
        {
            var image = _medicineService.GetMedicineImage(id);

            if (image == null)
                return NotFound();

            return File(image, "image/jpeg");
        }
    }
}
