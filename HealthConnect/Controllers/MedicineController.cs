namespace HealthConnect.Controllers
{
    public class MedicineController : Controller
    {
        private readonly MedicineRepository _medicine;
        public MedicineController(MedicineRepository medicine)
        {
            _medicine = medicine;
        }

        [HttpGet]
        public IActionResult SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<string>());
            }

            var suggestions = _medicine.GetMedicinesByPartialName(query);
            return Json(suggestions.Take(4));
        }

        [HttpGet]
        public IActionResult SearchMedicines()
        {
            return View();
        }
        public IActionResult DisplayMedicines(int categoryId)
        {
            var model = _medicine.getMedicines(categoryId);
            ViewBag.CategoryId = categoryId;
            return View(model);
        }
        [HttpGet]
        public IActionResult MedicineInfo(int id)
        {
            var medicine = _medicine.GetMedicineById(id);

            // If medicine is not found, return a "Not Found" page or handle appropriately
            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }
        [HttpGet]
        public IActionResult MedicineInfoByName(string medicineName)
        {
            if (string.IsNullOrWhiteSpace(medicineName))
            {
                return NotFound("Invalid medicine name.");
            }

            // Decoding URL by replace(-,' ') 
            var decodedName = medicineName.Replace('-', ' ');

            Medicine? medicine = _medicine.GetMedicineByName(decodedName).FirstOrDefault();

            if (medicine == null)
            {
                return NotFound("Medicine not found.");
            }

            return View("MedicineInfo", medicine);
        }

        [HttpGet]
        public IActionResult SearchbyProduct(string medicineName, int categoryId)
        {
            var medicines = _medicine.GetMedicineByName(medicineName, categoryId);
            ViewBag.CategoryId = categoryId;
            return View("DisplayMedicines", medicines);
        }

        [HttpGet("GetImage/{id}")]
        public IActionResult GetImage(int id)
        {
            var medicine = _medicine.GetMedicineById(id);

            if (medicine == null || medicine.Image == null)
            {
                return NotFound(); // Return 404 if the medicine or image is not found
            }

            return File(medicine.Image, "image/jpeg");
        }
    }
}
