using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthConnect.Models;
using HealthConnect.Repositories;
using System.Text.Json;

namespace HealthConnect.Controllers
{

    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IConfiguration _configuration;
        private const string apiKey = "AIzaSyDoQ2jdmGKmKALVj977-JCYZ7jUT6J6OHA";  // Use your Google API key
        private const string apiUrl = "https://maps.googleapis.com/maps/api/geocode/json";
        public UserController(UserManager<User> userManager,IDoctorRepository doctorRepository, IConfiguration configuration)
        {
            _userManager = userManager;
            _doctorRepository= doctorRepository;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> FindDoctors()
        {
            ViewData["GoogleMapsApiKey"] = _configuration["GoogleMaps:ApiKey"];
            //(double x,double y) = await GetCoordinatesAsync("India");
            //ViewBag.x = x; ViewBag.y = y;
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





        //public async Task<(double latitude, double longitude)> GetCoordinatesAsync(string address)
        //{
        //    using (HttpClient client = new HttpClient())
        //    {
        //        string requestUrl = $"{apiUrl}?address={Uri.EscapeDataString(address)}&key={apiKey}";
        //        HttpResponseMessage response = await client.GetAsync(requestUrl);
        //        string content = await response.Content.ReadAsStringAsync();

        //        // Parse JSON response using System.Text.Json
        //        using (JsonDocument doc = JsonDocument.Parse(content))
        //        {
        //            // Check if "results" array is empty or not
        //            if (doc.RootElement.TryGetProperty("results", out JsonElement results) && results.GetArrayLength() > 0)
        //            {
        //                // Access the first element of the "results" array
        //                var location = results[0].GetProperty("geometry").GetProperty("location");

        //                double latitude = location.GetProperty("lat").GetDouble();
        //                double longitude = location.GetProperty("lng").GetDouble();
        //                return (latitude, longitude);
        //            }
        //            else
        //            {
        //                // Handle the case where no results were found
        //                throw new Exception("No results found for the address.");
        //            }
        //        }
        //    }
        //}

        public IActionResult DoctorsNearYou()
        {
            return View();
        }

        //// GET: Edit Profile
        //[HttpGet]
        //public async Task<IActionResult> EditProfile()
        //{
        //    var user = await _userManager.GetUserAsync(User);  // Get logged-in user
        //    if (user == null)
        //    {
        //        return RedirectToAction("Login", "Account");  // Redirect to login if user not found
        //    }

        //    return View(user);  // Return the profile data to the view
        //}

        //// POST: Edit Profile
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditProfile(User updatedUser, IFormFile profilePhoto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        // Return the view with validation errors
        //        return View(updatedUser);
        //    }

        //    var user = await _userManager.GetUserAsync(User);  // Get logged-in user
        //    if (user == null)
        //    {
        //        return RedirectToAction("Login", "Account");  // Redirect to login if user not found
        //    }

        //    // Update the user properties
        //    user.UserName = updatedUser.UserName;
        //    user.DateofBirth = updatedUser.DateofBirth;
        //    user.Gender = updatedUser.Gender;
        //    user.BloodGroup = updatedUser.BloodGroup;
        //    user.HouseNumber = updatedUser.HouseNumber;
        //    user.Street = updatedUser.Street;
        //    user.City = updatedUser.City;
        //    user.State = updatedUser.State;
        //    user.Country = updatedUser.Country;
        //    user.PostalCode = updatedUser.PostalCode;

        //    // Handle profile photo upload
        //    if (profilePhoto != null && profilePhoto.Length > 0)
        //    {
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            await profilePhoto.CopyToAsync(memoryStream);
        //            user.ProfilePhoto = memoryStream.ToArray();  // Store the profile photo as byte array
        //        }
        //    }

        //    // Save the changes to the database
        //    var result = await _userManager.UpdateAsync(user);

        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction("Profile", "User");  // Redirect to profile page on success
        //    }

        //    // Handle errors (e.g., validation errors)
        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.AddModelError("", error.Description);
        //    }

        //    // Return the view with validation errors
        //    return Ok("Unsuucessful");
        //}

        //public async Task<IActionResult> GetProfilePhoto(string userId)
        //{
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user?.ProfilePhoto != null)
        //    {
        //        return File(user.ProfilePhoto, "image/jpeg");
        //    }

        //    // Return a default profile photo if none is set
        //    return File("~/images/download.png", "image/jpeg");
        //}

    }
}
