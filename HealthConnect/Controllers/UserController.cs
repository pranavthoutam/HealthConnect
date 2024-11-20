using HealthConnect.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HealthConnect.Controllers
{
    public class UserController : Controller
    {
        private readonly IDoctorRepository _doctorRepository;

        public UserController(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult FindDoctors()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FindDoctors(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                ViewBag.Message = "Please enter a valid search term.";
                return View(); // No results to display if search string is empty
            }

            // Fetch matching doctors based on name or specialization
            var doctors = await _doctorRepository.SearchDoctorsAsync(searchString);

            if (doctors == null || !doctors.Any())
            {
                ViewBag.Message = "No doctors found matching the search criteria.";
                return View();
            }

            return View(doctors); // Pass results to the view
        }

        public IActionResult DoctorsNearYou()
        {
            return View();
        }
    }
}
