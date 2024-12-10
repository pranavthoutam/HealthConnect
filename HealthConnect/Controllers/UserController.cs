namespace HealthConnect.Controllers
{
    public class UserController : Controller
    {
        private readonly DoctorService _doctorService;
        private readonly AppointmentService _appointmentService;
        private readonly FeedbackService _feedbackService;

        public UserController(DoctorService doctorService, AppointmentService appointmentService, FeedbackService feedbackService)
        {
            _doctorService = doctorService;
            _appointmentService = appointmentService;
            _feedbackService = feedbackService;
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

            return View(doctor);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(int doctorId, string selectedSlot, DateTime date, int isOnline, AppointmentStatus? status, int? appointmentId)
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
                    return View();
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
                Status = status ?? AppointmentStatus.Scheduled
            };

            await _appointmentService.AddAppointmentAsync(newAppointment);
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
