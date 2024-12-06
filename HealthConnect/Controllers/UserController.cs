namespace HealthConnect.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly DoctorRepository _doctorRepository;
        private readonly IConfiguration _configuration;
        private readonly FeedbackService _feedbackService;
        public UserController(UserManager<User> userManager, DoctorRepository doctorRepository, IConfiguration configuration, FeedbackService feedbackService)
        {
            _userManager = userManager;
            _doctorRepository = doctorRepository;
            _configuration = configuration;
            _feedbackService = feedbackService;
        }

        [HttpGet]
        //Finds the Doctors according to the location and Name or specialization of doctor
        public IActionResult FindDoctors()
        {
            return View();
        }
        [HttpPost]
        public IActionResult FindDoctors(DoctorFilterViewModel filter)
        {
            return RedirectToAction("DoctorsNearYou", new { location = filter.Location, searchString = filter.SearchString });
        }


        // Display doctors with filters applied (gender, experience)
        public IActionResult DoctorsNearYou(string location, string searchString, DoctorFilterViewModel filter)
        {
            var loggedInDoctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isDoctor = User.IsInRole("Doctor");

            var doctors = _doctorRepository.GetDoctorsByLocationAndSpecialization(location, searchString,isDoctor ? loggedInDoctorId : null);

            var filteredDoctors = _doctorRepository.ApplyFilters(doctors, filter);

            filter.Doctors = filteredDoctors.ToList();

            return View(filter);
        }

        /// <summary>
        /// Gets the Doctor Profile and his available slots for the consultation
        /// </summary>
        /// <param name="doctorId"></param>
        /// <param name="date"></param>
        /// <param name="isOnline"></param>
        /// <param name="status"></param>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> BookAppointment(int doctorId, DateTime date, int isOnline, AppointmentStatus? status, int? appointmentId)
        {
            var doctor = await _doctorRepository.SearchDoctorAsync(doctorId);
            if (doctor == null)
            {
                TempData["Message"] = "Doctor not found.";
                return RedirectToAction("FindDoctors");
            }

            var availableSlots = await _doctorRepository.GetAvailableSlotsAsync(doctorId, date);


            ViewBag.Feedbacks = _feedbackService.GetDoctorFeedBacks(doctorId);
            ViewBag.AvailableSlots = availableSlots.ToList();
            ViewBag.SelectedDate = date;
            ViewBag.IsOnline = isOnline;

            if (status == AppointmentStatus.ReScheduled) ViewBag.AppointmentStatus = status;
            else ViewBag.AppointmentStatus = AppointmentStatus.Scheduled;
            if (appointmentId != null) ViewBag.AppointmentId = appointmentId;
            ViewData["Title"] = "Book Appointment";
            return View(doctor);
        }

        //Confirm Appointment
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(int doctorId, string selectedSlot, DateTime date, int isOnline, AppointmentStatus? status, int? appointmentId)
        {
            if (string.IsNullOrEmpty(selectedSlot))
            {
                TempData["Message"] = "Please select a valid time slot.";
                return RedirectToAction("BookAppointment", new { doctorId, date });
            }

            bool consultationType = isOnline == 1;
            string meetingLink = null;

            // Generate a meeting link if the consultation is online
            if (consultationType)
            {
                string roomName = $"healthconnect-room-{doctorId}-{User.FindFirstValue(ClaimTypes.NameIdentifier)}-{Guid.NewGuid()}";
                meetingLink = $"https://meet.jit.si/{roomName}";
                ViewBag.MeetingLink = meetingLink;
            }

            // Handle rescheduling if appointmentId is provided
            if (appointmentId != null)
            {
                var existingAppointment = await _doctorRepository.GetAppointmentByIdAsync((int)appointmentId);
                if (existingAppointment != null)
                {
                    existingAppointment.AppointmentDate = date;
                    existingAppointment.Slot = selectedSlot;
                    existingAppointment.IsOnline = consultationType;
                    existingAppointment.ConsultationLink = meetingLink;
                    existingAppointment.Status = status ?? AppointmentStatus.ReScheduled;

                    await _doctorRepository.RescheduleAppointment(existingAppointment);

                    TempData["Message"] = "Appointment rescheduled successfully.";
                    ViewBag.TimeSlot = selectedSlot;
                    ViewBag.AppointmentDate = date;
                    return RedirectToAction("ProfileDashboard", "Account");
                }
                else
                {
                    TempData["Message"] = "Appointment not found for rescheduling.";
                    return RedirectToAction("ProfileDashboard", "Account");
                }
            }

            // Check for duplicate appointments
            var duplicateAppointment = await _doctorRepository.GetAppointmentByDetailsAsync(doctorId, User.FindFirstValue(ClaimTypes.NameIdentifier), date, selectedSlot);
            if (duplicateAppointment != null)
            {
                TempData["Message"] = "You already have an appointment for this time slot.";
                return RedirectToAction("ProfileDashboard", "Account");
            }

            // Create a new appointment
            var newAppointment = new Appointment
            {
                DoctorId = doctorId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Slot = selectedSlot,
                AppointmentDate = date,
                IsOnline = consultationType,
                ConsultationLink = meetingLink,
                Status = status ?? AppointmentStatus.Scheduled
            };

            await _doctorRepository.AddAppointmentAsync(newAppointment);

            TempData["Message"] = "Appointment booked successfully!";
            ViewBag.TimeSlot = selectedSlot;
            ViewBag.AppointmentDate = date;
            return View();
        }



        //Cancel Appointment
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var appointment = await _doctorRepository.GetAppointmentByIdAsync(appointmentId);

            if (appointment == null || appointment.AppointmentDate.Add(TimeSpan.Parse(appointment.Slot)).Subtract(DateTime.Now).TotalHours < 3)
            {
                TempData["Error"] = "Appointment cannot be canceled within 3 hours of the scheduled time.";
                return RedirectToAction("ProfileDashboard", "Account");
            }

            await _doctorRepository.CancelAppointmentAsync(appointment);
            appointment.Status = AppointmentStatus.Canceled;
            TempData["Success"] = "Appointment canceled successfully.";
            return View();


        }


        // GET: EditProfile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);  // Get logged-in user
            if (user == null)
            {
                return NotFound();  // If user not found, return 404
            }

            var model = new UserProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                BloodGroup = user.BloodGroup,
                DateofBirth = user.DateofBirth,
                HouseNumber = user.HouseNumber,
                Street = user.Street,
                City = user.City,
                State = user.State,
                Country = user.Country,
                PostalCode = user.PostalCode,
            };

            // Pass profile photo (as byte array) to the view for previewing
            if (user.ProfilePhoto != null)
            {
                ViewBag.ProfilePhoto = user.ProfilePhoto;  // Store it in ViewBag
            }

            return View(model);
        }

        // POST: EditProfile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UserProfileViewModel updatedUser)
        {
            if (!ModelState.IsValid)
            {
                // Return the view with validation errors
                return View(updatedUser);
            }

            User? user = await _userManager.GetUserAsync(User);  // Get logged-in user
            if (user == null)
            {
                return NotFound();  // If user not found, return 404
            }

            // Update the user properties from the view model
            user.UserName = updatedUser.UserName;
            user.DateofBirth = updatedUser.DateofBirth;
            user.Gender = updatedUser.Gender;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.BloodGroup = updatedUser.BloodGroup;
            user.HouseNumber = updatedUser.HouseNumber;
            user.Street = updatedUser.Street;
            user.City = updatedUser.City;
            user.State = updatedUser.State;
            user.Country = updatedUser.Country;
            user.PostalCode = updatedUser.PostalCode;

            // Handle profile photo upload (convert IFormFile to byte array)
            if (updatedUser.ProfilePhoto != null && updatedUser.ProfilePhoto.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await updatedUser.ProfilePhoto.CopyToAsync(memoryStream);
                    user.ProfilePhoto = memoryStream.ToArray();  // Store the profile photo as byte array
                }
            }

            // Save the changes to the database
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");  // Redirect to the profile page on success
            }

            // Handle errors (e.g., validation errors)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            // Return the view with validation errors
            return View(updatedUser);
        }


        public async Task<IActionResult> GetProfilePhoto(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user?.ProfilePhoto != null)
            {
                return File(user.ProfilePhoto, "image/jpeg");
            }

            return File("~/images/download.png", "image/jpeg");
        }
    }
}