using HealthConnect.Models;
using HealthConnect.Services;
using HealthConnect.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthConnect.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailService _emailService;

        private static string Otp;
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

        // Forgot Password (POST)
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email not found.");
                    return View(model);
                }

                // Generate OTP
                Otp = new Random().Next(100000, 999999).ToString();
                
                // Send OTP via email
                await _emailService.SendEmailAsync(user.Email, "Password Reset OTP", $"Your OTP is {Otp}");

                // Store email temporarily
                TempData["Email"] = model.Email;

                return RedirectToAction("VerifyOtp");
            }
            return View(model);
        }

        // Verify OTP (GET)
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (TempData["Email"] == null)
                return RedirectToAction("ForgotPassword");

            TempData.Keep("Email");
            return View();
        }

        // Verify OTP (POST)
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (TempData["Email"] == null)
                return RedirectToAction("ForgotPassword");

            string email = TempData["Email"].ToString();
            TempData.Keep("Email");
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || Otp != model.OTP || user.OTPExpiryTime < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Invalid or expired OTP.");
                return View();
            }
            if (ModelState.IsValid) { 

                return RedirectToAction("ResetPassword");
            }
            return View();
        }

        // Reset Password (GET)
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["Email"] == null)
                return RedirectToAction("ForgotPassword");

            TempData.Keep("Email");
            return View();
        }

        // Reset Password (POST)
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (TempData["Email"] == null)
                return RedirectToAction("ForgotPassword");

            string email = TempData["Email"].ToString();
            model.Email = email;

            if (model.NewPassword!=null && model.ConfirmPassword!=null && model.NewPassword==model.ConfirmPassword)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email.");
                    return View(model);
                }

                // Reset password
                var resetResult = await _userManager.RemovePasswordAsync(user);
                if (resetResult.Succeeded)
                {
                    await _userManager.AddPasswordAsync(user, model.NewPassword);
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (var error in resetResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }


        public IActionResult Profile()
        {
            // Add logic to display user profile details
            return View();
        }
    }
}
