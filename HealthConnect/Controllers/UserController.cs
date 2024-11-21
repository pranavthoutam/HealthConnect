using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthConnect.Models;
using HealthConnect.Repositories;

namespace HealthConnect.Controllers
{

    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IDoctorRepository _doctorRepository;
        public UserController(UserManager<User> userManager,IDoctorRepository doctorRepository)
        {
            _userManager = userManager;
            _doctorRepository= doctorRepository;
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
