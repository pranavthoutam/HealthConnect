using HealthConnect.Models;
using HealthConnect.Services;
using HealthConnect.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace HealthConnect.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailService _emailService;

        private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expiration)> _otps = new();
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager,EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

       
        // Create roles if they don't exist
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

        // Index view
        public IActionResult Index()
        {
            return View();
        }

        // Register view (GET)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Register view (POST)
        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserViewModel userViewModel)
        {
            await CreateRoles();
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = userViewModel.UserName,
                    Email = userViewModel.Email,
                };

                var createUser = await _userManager.CreateAsync(user, userViewModel.Password);
                if (createUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, userViewModel.Role);
                    if (roleResult.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }

                foreach (var error in createUser.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(userViewModel);
        }

        // Login view (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Login view (POST)
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == loginViewModel.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, loginViewModel.RememberMe, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return View(loginViewModel);
        }
        [HttpGet]
        public IActionResult DoctorRegister()
        {
            return View();
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, string otp = null)
        {
            // If no TempData Email exists, we are in the first step (sending OTP)
            if (TempData["Email"] == null)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "Email not found.");
                        return View();
                    }

                    // Generate OTP
                    string generatedOtp = new Random().Next(100000, 999999).ToString();
                    DateTime expiration = DateTime.UtcNow.AddMinutes(3);

                    // Store OTP with expiration
                    _otps[model.Email] = (generatedOtp, expiration);

                    // Send OTP via email
                    await _emailService.SendEmailAsync(model.Email, "Password Reset OTP", $"Your OTP is {generatedOtp}. It will expire in 3 minutes.");

                    TempData["Email"] = model.Email;
                    TempData.Keep("Email");
                    return View();
                }
            }
            else
            {
                // Email already provided, validate OTP
                string email = TempData["Email"].ToString();
                TempData.Keep("Email");

                if (string.IsNullOrEmpty(otp))
                {
                    ModelState.AddModelError("", "Please enter the OTP.");
                    return View();
                }

                // Check if OTP is valid and not expired
                if (_otps.TryGetValue(email, out var otpDetails))
                {
                    if (otpDetails.Otp == otp && otpDetails.Expiration > DateTime.UtcNow)
                    {
                        // OTP verified, remove from store
                        _otps.TryRemove(email, out _);

                        // Redirect to reset password
                        TempData["Email"] = email;
                        return RedirectToAction("ResetPassword");
                    }
                }

                ModelState.AddModelError("", "Invalid or expired OTP.");
            }

            return View();
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
                var user = await _userManager.FindByEmailAsync(email);
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
                    return RedirectToAction("Login");
                }

                foreach (var error in resetResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User); // Get the logged-in user's ID
            if (userId == null)
            {
                return RedirectToAction("Login"); // Redirect to login if not authenticated
            }

            var user = await _userManager.FindByIdAsync(userId); // Fetch the user details
            if (user == null)
            {
                return NotFound(); // Handle case if user is not found
            }

            return View(user); // Pass the user object to the view
        }

        public IActionResult ProfileDashboard()
        {
            return View();
        }

    }
}
