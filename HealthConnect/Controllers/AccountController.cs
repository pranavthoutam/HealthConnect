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

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserViewModel userViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(userViewModel);
            }

            User user = new User
            {
                UserName = userViewModel.UserName,
                Email = userViewModel.Email,
            };

            var createUser = await _userManager.CreateAsync(user, userViewModel.Password);

            if (createUser.Succeeded) RedirectToAction("Index", "Home");

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
            if (user!=null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.UserName, loginViewModel.Password, loginViewModel.RememberMe, false);
                
                if (result.Succeeded) return RedirectToAction("Index", "Home");
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
                    User? user = await _userManager.FindByEmailAsync(model.Email);
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
                        _otps.TryRemove(email, out _);

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
                    TempData.Remove("Email");
                    return RedirectToAction("Login");
                }

                foreach (var error in resetResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> ProfileDashboard()
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            IEnumerable<Appointment> appointments = await _doctorRepository.GetAppointmentsForUserAsync(userId);

            ViewBag.UserId = userId;

            User user = await _userManager.FindByIdAsync(userId);
            List<Feedback> feedbacks = (List<Feedback>)await _doctorRepository.GetFeedBacksAsync(userId);

            // Process appointments
            DateTime currentTime = DateTime.Now;
            ProfileDashboardViewModel viewModel = new ProfileDashboardViewModel
            {
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePhoto = user.ProfilePhoto,
                Name = user.UserName,
                Feedbacks = feedbacks,
                InClinicAppointments = appointments
                                       .Where(a => a.IsOnline == false)
                                       .Select(a => new AppointmentViewModel
                                       {
                                           AppointmentId = a.Id,
                                           DoctorName = a.Doctor.FullName,
                                           DoctorId = a.Doctor.Id,
                                           DoctorSpecialization = a.Doctor.Specialization,
                                           AppointmentDate = a.AppointmentDate,
                                           Slot = a.Slot,
                                           Location = a.Doctor.Place,
                                           CanRescheduleOrCancel = a.AppointmentDate.Add(TimeSpan.Parse(a.Slot)).Subtract(currentTime).TotalHours > 3,
                                           IsCompleted = a.AppointmentDate.Date < currentTime.Date ||
                                                        (a.AppointmentDate.Date == currentTime.Date && TimeSpan.Parse(a.Slot) < currentTime.TimeOfDay),
                                       })
                                        .ToList(),


                OnlineConsultations = appointments
                                        .Where(a => a.IsOnline == true)
                                        .Select(a => new AppointmentViewModel
                                        {
                                            AppointmentId = a.Id,
                                            DoctorName = a.Doctor.FullName,
                                            DoctorId = a.Doctor.Id,
                                            DoctorSpecialization = a.Doctor.Specialization,
                                            AppointmentDate = a.AppointmentDate,
                                            Slot = a.Slot,
                                            Location = "Online",
                                            CanRescheduleOrCancel = a.AppointmentDate.Add(TimeSpan.Parse(a.Slot)).Subtract(currentTime).TotalHours > 3,
                                            IsCompleted = a.AppointmentDate.Date < currentTime.Date ||
                                                            (a.AppointmentDate.Date == currentTime.Date && TimeSpan.Parse(a.Slot) < currentTime.TimeOfDay),
                                            MeetingLink=a.ConsultationLink
                                        })
                                        .ToList(),

                CompletedAppointments = appointments
                                        .Where(a => a.AppointmentDate.Date < currentTime.Date ||
                                            (a.AppointmentDate.Date == currentTime.Date && TimeSpan.Parse(a.Slot) <= currentTime.TimeOfDay))
                                        .Select(a => new AppointmentViewModel
                                        {
                                            AppointmentId = a.Id,
                                            DoctorName = a.Doctor.FullName,
                                            DoctorId = a.Doctor.Id,
                                            DoctorSpecialization = a.Doctor.Specialization,
                                            AppointmentDate = a.AppointmentDate,
                                            Slot = a.Slot,
                                            Location = a.Doctor.Place ?? "Online"
                                        })
                                        .ToList()
            };

            return View(viewModel);
        }

    }
}
