namespace HealthConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult StatusCode(int code)
        {
            _logger.LogWarning($"Error to load Page {code}");
            if (code == 404)
            {
                return View("NotFound"); // The custom 404 view
            }

            return View("Error"); 
        }
    }
}
