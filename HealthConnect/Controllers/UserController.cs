namespace HealthConnect.Controllers
{
    public class UserController : Controller
    {
        private readonly DoctorService _doctorService;
        private readonly AppointmentService _appointmentService;
        private readonly FeedbackService _feedbackService;
        private readonly EmailService _emailService;
        private readonly EmailScheduler _emailScheduler;

        public UserController(DoctorService doctorService, AppointmentService appointmentService,
                              FeedbackService feedbackService,EmailService emailService,
                              EmailScheduler emailScheduler)
        {
            _doctorService = doctorService;
            _appointmentService = appointmentService;
            _feedbackService = feedbackService;
            _emailService = emailService;
            _emailScheduler = emailScheduler;
        }

        [HttpGet]
        public IActionResult FindDoctors()
        {
            return View();
        }

        [HttpPost]
        public IActionResult FindDoctors(DoctorFilterViewModel filter)
        {
            return RedirectToAction("DoctorsNearYou", new { location = filter.Location, searchString = filter.SearchString });
        }

        public IActionResult DoctorsNearYou(string location, string searchString, DoctorFilterViewModel filter)
        {
            var loggedInDoctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctors = _doctorService.FindDoctors(location, searchString, User.IsInRole("Doctor") ? loggedInDoctorId : null);
            var filteredDoctors = _doctorService.ApplyFilters(doctors, filter);
            filter.Doctors = filteredDoctors.ToList();
            return View(filter);
        }

        [HttpGet]
        public async Task<IActionResult> BookAppointment(int doctorId, DateTime date, int isOnline, AppointmentStatus? status, int? appointmentId)
        {
            var doctor = await _doctorService.GetDoctorProfileAsync(doctorId);
            if (doctor == null)
            {
                TempData["Message"] = "Doctor not found.";
                return RedirectToAction("FindDoctors");
            }

            var availableSlots = await _doctorService.GetAvailableSlotsAsync(doctorId, date);

            ViewBag.Feedbacks = _feedbackService.GetDoctorFeedBacks(doctorId);
            ViewBag.AvailableSlots = availableSlots.ToList();
            ViewBag.SelectedDate = date;
            ViewBag.IsOnline = isOnline;
            ViewBag.AppointmentStatus = status ?? AppointmentStatus.Scheduled;
            ViewBag.AppointmentId = appointmentId;
            ViewBag.ConsultationFee = doctor.ConsultationFee;
            ViewBag.DoctorName = doctor.FullName;

            return View(doctor);
        }

        [Authorize]
        [HttpPost]
        public IActionResult PatientDetails(int doctorId, string doctorName, DateTime date,
                                            string selectedSlot, int isOnline, decimal consultationFee,
                                            int? appointmentId ,AppointmentStatus? status)
        {
            ViewBag.DoctorId = doctorId;
            ViewBag.SelectedDate = date;
            ViewBag.SelectedSlot = selectedSlot;
            ViewBag.IsOnline = isOnline;
            ViewBag.AppointmentStatus = status ?? AppointmentStatus.Scheduled;
            ViewBag.AppointmentId = appointmentId;
            ViewBag.DoctorName = doctorName;
            ViewBag.ConsultationFee = consultationFee;
            return View();
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(
    int doctorId, string selectedSlot, DateTime date,
    int isOnline, string patientName, string healthConcern,
    AppointmentStatus? status, int? appointmentId)
        {
            if (string.IsNullOrEmpty(selectedSlot))
            {
                TempData["Message"] = "Please select a valid time slot.";
                return RedirectToAction("BookAppointment", new { doctorId, date });
            }

            string consultationLink = isOnline == 1
                ? $"https://meet.jit.si/healthconnect-room-{Guid.NewGuid()}#config.disableModerator=true"
                : null;

            if (appointmentId != null)
            {
                if (await _appointmentService.RescheduleAppointmentAsync((int)appointmentId, date, selectedSlot, isOnline == 1, consultationLink))
                {
                    TempData["Message"] = "Appointment rescheduled successfully.";
                    return RedirectToAction("ProfileDashboard", "UserProfile");
                }

                TempData["Message"] = "Appointment not found for rescheduling.";
                return RedirectToAction("ProfileDashboard", "UserProfile");
            }

            if (await _appointmentService.IsDuplicateAppointmentAsync(doctorId, User.FindFirstValue(ClaimTypes.NameIdentifier), date, selectedSlot))
            {
                TempData["Message"] = "You already have an appointment for this time slot.";
                return RedirectToAction("ProfileDashboard", "UserProfile");
            }

            var newAppointment = new Appointment
            {
                DoctorId = doctorId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Slot = selectedSlot,
                AppointmentDate = date,
                IsOnline = isOnline == 1,
                ConsultationLink = consultationLink,
                Status = status ?? AppointmentStatus.Scheduled,
                PatientName = patientName,
                HealthConcern = healthConcern,
            };

            await _appointmentService.AddAppointmentAsync(newAppointment);

            // Schedule emails
            string userEmail = User.FindFirstValue(ClaimTypes.Email);
            string confirmationSubject = isOnline == 1
                ? "Online Appointment Confirmation"
                : "In-Clinic Appointment Confirmation";
            string confirmationBody = isOnline == 1
                ? $"Your appointment has been scheduled successfully. You will receive the meeting link 10 minutes before your scheduled time."
                : $"Your in-clinic appointment has been scheduled successfully for {date.ToShortDateString()} at {selectedSlot}.";

            _emailScheduler.ScheduleEmail(userEmail, confirmationSubject, confirmationBody, DateTime.Now.AddSeconds(1));
            if (TimeSpan.TryParse(selectedSlot, out var slotTime))
            {
                var appointmentDateTime = date.Date.Add(slotTime);

                if (isOnline == 1)
                {
                    string reminderSubject = "Your Online Appointment Reminder";
                    string reminderBody = $"Your appointment starts in 10 minutes. Use this link: {consultationLink}";
                    _emailScheduler.ScheduleEmail(userEmail, reminderSubject, reminderBody, appointmentDateTime.AddMinutes(-10));
                }
                else
                {
                    string reminderSubject = "Your In-Clinic Appointment Reminder";
                    string reminderBody = $"Your in-clinic appointment starts in 30 minutes at {appointmentDateTime.ToString("f")}.";
                    _emailScheduler.ScheduleEmail(userEmail, reminderSubject, reminderBody, appointmentDateTime.AddMinutes(-30));
                }
            }
            else
            {
                TempData["Error"] = "Invalid time slot format.";
            }

            ViewBag.AppointmentDate = date.ToShortDateString();
            ViewBag.TimeSlot = selectedSlot;
            TempData["Message"] = "Appointment booked successfully!";
            ViewBag.MeetingLink = consultationLink;
            return View();
        }



        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            if (await _appointmentService.CancelAppointmentAsync(appointmentId))
            {
                TempData["Success"] = "Appointment canceled successfully.";
            }
            else
            {
                TempData["Error"] = "Appointment cannot be canceled within 3 hours of the scheduled time.";
            }

            return View();
        }
    }
}
