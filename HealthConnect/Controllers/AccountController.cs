namespace HealthConnect.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailService _emailService;
        private readonly DoctorRepository _doctorRepository;

        private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expiration)> _otps = new();
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, EmailService emailService, DoctorRepository doctorRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _doctorRepository = doctorRepository;
        }


        public async Task<IActionResult> CreateRoles()
        {
            string[] roleNames = { "Admin", "Doctor", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            return Ok("Roles created successfully.");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserViewModel userViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(userViewModel);
            }

            var existingUser = await _userManager.FindByNameAsync(userViewModel.UserName);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(userViewModel.UserName), "Username is already taken.");
                return View(userViewModel);
            }

            User user = new User
            {
                UserName = userViewModel.UserName,
                Email = userViewModel.Email,
            };

            var createUser = await _userManager.CreateAsync(user, userViewModel.Password);
            var userRole = await _userManager.AddToRoleAsync(user, userViewModel.Role);
            if (createUser.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in createUser.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(userViewModel);
        }


        // Login view (GET)
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // Login view (POST)
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }

            User user = await _userManager.FindByEmailAsync(loginViewModel.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.UserName, loginViewModel.Password, loginViewModel.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("Password", "Incorrect password. Please try again.");
            }
            else
            {
                ModelState.AddModelError("Email", "No user found with this email address.");
            }

            return View(loginViewModel);
        }
        [HttpGet]
        public IActionResult DoctorRegister()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, string otp = null)
        {
            if (model.Email == null)
            {
                if (ModelState.IsValid)
                {
                    User? user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "Email not found.");
                        return View();
                    }


                    // TempData.Keep("Email");
                    return View();
                }
            }
            else
            {
                string email = model.Email;
                // TempData.Keep("Email");

                if (string.IsNullOrEmpty(otp))
                {
                    string generatedOtp = new Random().Next(100000, 999999).ToString();
                    DateTime expiration = DateTime.UtcNow.AddMinutes(3);
                    _otps[model.Email] = (generatedOtp, expiration);

                    await _emailService.SendEmailAsync(model.Email, "Password Reset OTP", $"Your OTP is {generatedOtp}. It will expire in 3 minutes.");
                    TempData["Email"] = model.Email;
                    ModelState.AddModelError("", "Please enter the OTP.");
                    return View(model);
                }

                if (_otps.TryGetValue(email, out var otpDetails))
                {
                    if (otpDetails.Otp == otp && otpDetails.Expiration > DateTime.UtcNow)
                    {
                        _otps.TryRemove(email, out _);

                        TempData["Email"] = email;
                        return RedirectToAction("ResetPassword");
                    }
                }

                ModelState.AddModelError("", "Invalid or expired OTP.");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return Json(new { success = false, message = "Invalid email." });
            }

            if (_otps.ContainsKey(model.Email))
            {
                string generatedOtp = new Random().Next(100000, 999999).ToString();
                DateTime expiration = DateTime.UtcNow.AddMinutes(3);
                _otps[model.Email] = (generatedOtp, expiration);

                await _emailService.SendEmailAsync(model.Email, "Password Reset OTP", $"Your new OTP is {generatedOtp}. It will expire in 3 minutes.");
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Email not found." });
        }


        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["Email"] == null)
                return RedirectToAction("ForgotPassword");

            TempData.Keep("Email");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (TempData["Email"] == null)
                return RedirectToAction("ForgotPassword");

            var email = TempData["Email"].ToString();

            if (ModelState.IsValid && model.NewPassword == model.ConfirmPassword)
            {
                User? user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email.");
                    return View(model);
                }

                // Reset the password
                var resetResult = await _userManager.RemovePasswordAsync(user);
                if (resetResult.Succeeded)
                {
                    await _userManager.AddPasswordAsync(user, model.NewPassword);
                    //TempData.Remove("Email");
                    return RedirectToAction("Login");
                }

                foreach (var error in resetResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

    }
}
