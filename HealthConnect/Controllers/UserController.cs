namespace HealthConnect.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly DoctorRepository _doctorRepository;
        private readonly IConfiguration _configuration;
        public UserController(UserManager<User> userManager, DoctorRepository doctorRepository, IConfiguration configuration)
        {
            _userManager = userManager;
            _doctorRepository = doctorRepository;
            _configuration = configuration;
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
            var doctors = _doctorRepository.GetDoctorsByLocationAndSpecialization(location, searchString);

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

            if (date.Date == DateTime.Now.Date)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                availableSlots = availableSlots
                    .Where(slot =>
                    {
                        if (TimeSpan.TryParse(slot, out var slotTime))
                        {
                            return slotTime > currentTime;
                        }
                        return false;
                    })
                    .ToList();
            }

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
        //[Authorize]
        //[HttpPost]
        //public async Task<IActionResult> ConfirmAppointment(int doctorId, string selectedSlot, DateTime date, int isOnline, AppointmentStatus? status, int? appointmentId)
        //{
        //    if (string.IsNullOrEmpty(selectedSlot))
        //    {
        //        TempData["Message"] = "Please select a valid time slot.";
        //        return RedirectToAction("BookAppointment", new { doctorId, date });
        //    }

        //    var consultationType = isOnline == 1;
        //    string meetingLink = null;

        //    if (consultationType)
        //    {
        //        var roomName = $"healthconnect-room-{doctorId}-{User.FindFirstValue(ClaimTypes.NameIdentifier)}-{Guid.NewGuid()}";
        //        var startTime = date.Add(TimeSpan.Parse(selectedSlot)).AddMinutes(-5); // 5 minutes before
        //        var expiryTime = startTime.AddHours(1); // 1 hour after start time

        //        meetingLink = JitsiJwtService.GenerateJitsiMeetingLink(roomName, startTime, expiryTime,_userManager,User);
        //        ViewBag.MeetingLink = meetingLink;
        //    }

        //    if (appointmentId != null)
        //    {
        //        var existingAppointment = await _doctorRepository.GetAppointmentByIdAsync((int)appointmentId);
        //        existingAppointment.AppointmentDate = date;
        //        existingAppointment.Slot = selectedSlot;
        //        existingAppointment.ConsultationLink = meetingLink;
        //        await _doctorRepository.RescheduleAppointment(existingAppointment);

        //        return RedirectToAction("ProfileDashboard", "Account");
        //    }

        //    var newAppointment = new Appointment
        //    {
        //        DoctorId = doctorId,
        //        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
        //        Slot = selectedSlot,
        //        AppointmentDate = date,
        //        IsOnline = consultationType,
        //        ConsultationLink = meetingLink
        //    };

        //    if (status == AppointmentStatus.Scheduled)
        //        await _doctorRepository.AddAppointmentAsync(newAppointment);
        //    else
        //        await _doctorRepository.RescheduleAppointment(newAppointment);

        //    TempData["Message"] = status == null ? "Appointment booked successfully!" : "Appointment rescheduled successfully.";
        //    return View();
        //}
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(int doctorId, string selectedSlot, DateTime date, int isOnline, AppointmentStatus? status, int? appointmentId)
        {
            if (string.IsNullOrEmpty(selectedSlot))
            {
                TempData["Message"] = "Please select a valid time slot.";
                return RedirectToAction("BookAppointment", new { doctorId, date });
            }

            var consultationType = isOnline == 1;
            string meetingLink = null;

            if (consultationType)
            {
                var roomName = $"healthconnect-room-{doctorId}-{User.FindFirstValue(ClaimTypes.NameIdentifier)}-{Guid.NewGuid()}";
                var startTime = date.Add(TimeSpan.Parse(selectedSlot)).AddMinutes(-5); // 5 minutes before
                var expiryTime = startTime.AddHours(1); // 1 hour after start time

                // Generate Daily.co meeting link
                meetingLink = await DailyCoService.GenerateDailyRoomLink(roomName, startTime, expiryTime);
                ViewBag.MeetingLink = meetingLink;
            }

            if (appointmentId != null)
            {
                var existingAppointment = await _doctorRepository.GetAppointmentByIdAsync((int)appointmentId);
                existingAppointment.AppointmentDate = date;
                existingAppointment.Slot = selectedSlot;
                existingAppointment.ConsultationLink = meetingLink;
                await _doctorRepository.RescheduleAppointment(existingAppointment);

                return RedirectToAction("ProfileDashboard", "Account");
            }

            var newAppointment = new Appointment
            {
                DoctorId = doctorId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Slot = selectedSlot,
                AppointmentDate = date,
                IsOnline = consultationType,
                ConsultationLink = meetingLink
            };

            if (status == AppointmentStatus.Scheduled)
                await _doctorRepository.AddAppointmentAsync(newAppointment);
            else
                await _doctorRepository.RescheduleAppointment(newAppointment);

            TempData["Message"] = status == null ? "Appointment booked successfully!" : "Appointment rescheduled successfully.";
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



        // GET: Edit Profile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);  // Get logged-in user
            if (user == null)
            {
                return RedirectToAction("Login", "Account");  // Redirect to login if user not found
            }

            return View(user);  // Return the profile data to the view
        }

        // POST: Edit Profile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User updatedUser, IFormFile profilePhoto)
        {
            if (!ModelState.IsValid)
            {
                // Return the view with validation errors
                return View(updatedUser);
            }

            User? user = await _userManager.GetUserAsync(User);  // Get logged-in user
            

            // Update the user properties
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

            // Handle profile photo upload
            if (profilePhoto != null && profilePhoto.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await profilePhoto.CopyToAsync(memoryStream);
                    user.ProfilePhoto = memoryStream.ToArray();  // Store the profile photo as byte array
                }
            }

            // Save the changes to the database
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");  // Redirect to profile page on success
            }

            // Handle errors (e.g., validation errors)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            // Return the view with validation errors
            return Ok("Unsuucessful");
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