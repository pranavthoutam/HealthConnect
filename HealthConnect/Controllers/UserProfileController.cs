namespace HealthConnect.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly IUserProfileService _userProfileService;

        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [Authorize]
        public async Task<IActionResult> ProfileDashboard()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var dashboard = await _userProfileService.GetDashboardAsync(userId);
            if (dashboard == null)
            {
                return NotFound();
            }

            ViewBag.UserId = userId;
            return View(dashboard);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userProfile = await _userProfileService.GetUserProfileAsync(userId);

            if (userProfile == null)
            {
                return NotFound();
            }

            // Pass the profile photo as a byte array to the view
            ViewBag.ProfilePhoto = await _userProfileService.GetProfilePhotoAsync(userId);

            return View(userProfile);
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UserProfileViewModel updatedUser)
        {
            if (!ModelState.IsValid)
            {
                return View(updatedUser);
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _userProfileService.UpdateUserProfileAsync(userId, updatedUser);
            if (result.Succeeded)
            {
                return RedirectToAction("Index","Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(updatedUser);
        }

        public async Task<IActionResult> GetProfilePhoto(string userId)
        {
            var photo = await _userProfileService.GetProfilePhotoAsync(userId);
            if (photo != null)
            {
                return File(photo, "image/jpeg");
            }

            return File("~/images/download.png", "image/jpeg");
        }
    }
}
