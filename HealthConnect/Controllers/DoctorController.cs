using Microsoft.AspNetCore.Mvc;

namespace HealthConnect.Controllers
{
    public class DoctorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
