namespace HealthConnect.Controllers
{
    [ApiController]
    [Route("api")]
    public class SearchController : ControllerBase
    {
        private readonly List<string> Locations = new List<string> { "Madhapur", "Gachibowli", "Uppal", "Kukatpally", "LB Nagar" };
        private readonly List<string> Items = new List<string> { "Cardiology", "Dermatology", "General Physician" , "Pediatrics" };

        [HttpGet("locations")]
        public IActionResult GetLocations([FromQuery] string query)
        {
            var suggestions = Locations.Where(l => l.StartsWith(query, StringComparison.OrdinalIgnoreCase)).ToList();
            return Ok(suggestions);
        }

        [HttpGet("search-suggestions")]
        public IActionResult GetSearchSuggestions([FromQuery] string query)
        {
            var suggestions = Items.Where(i => i.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            return Ok(suggestions);
        }
    }
}
