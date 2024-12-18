namespace HealthConnect.Controllers
{
    public class UserController : Controller
    {
        private readonly DoctorService _doctorService;
        private readonly AppointmentService _appointmentService;
        private readonly FeedbackService _feedbackService;
        private readonly EmailService _emailService;
        private readonly EmailScheduler _emailScheduler;
        private readonly ClinicService _clinicService;

        public UserController(DoctorService doctorService, AppointmentService appointmentService,
                              FeedbackService feedbackService,EmailService emailService,
                              EmailScheduler emailScheduler, ClinicService clinicService)
        {
            _doctorService = doctorService;
            _appointmentService = appointmentService;
            _feedbackService = feedbackService;
            _emailService = emailService;
            _emailScheduler = emailScheduler;
            _clinicService = clinicService;
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
        public async Task<IActionResult> BookAppointment(int doctorId, DateTime date, int isOnline, AppointmentStatus? status, int? appointmentId, int? selectedClinicId)
        {
            var doctor = await _doctorService.GetDoctorProfileAsync(doctorId);
            if (doctor == null)
            {
                TempData["Message"] = "Doctor not found.";
                return RedirectToAction("FindDoctors");
            }

            IEnumerable<string> availableSlots;

            if (isOnline == 1)
            {
                availableSlots = await _doctorService.GetAvailableOnlineSlotsAsync(doctorId, date);
                selectedClinicId = null;
            }
            else 
            {
                if (selectedClinicId == null)
                {
                    TempData["Message"] = "Please select a clinic.";
                    return RedirectToAction("BookAppointment", new { doctorId, date, isOnline });
                }

                availableSlots = await _doctorService.GetAvailableSlotsAsync(doctorId, date, selectedClinicId.Value);
            }

            var clinics = await _clinicService.GetClinicsAsync(doctorId);
            var selectedClinic = clinics.FirstOrDefault(c => c.ClinicId == selectedClinicId) ?? clinics.FirstOrDefault(); // Default to first clinic if none selected

            var viewModel = new BookDoctorAppointmentViewModel
            {
                Doctor = doctor,
                SelectedClinicId = selectedClinic?.ClinicId,
                Clinics = clinics,
                SelectedClinic = selectedClinic
            };

            ViewBag.Feedbacks = _feedbackService.GetDoctorFeedBacks(doctorId);
            ViewBag.AvailableSlots = availableSlots.ToList();
            ViewBag.SelectedDate = date;
            ViewBag.IsOnline = isOnline;
            ViewBag.AppointmentStatus = status ?? AppointmentStatus.Scheduled;
            ViewBag.AppointmentId = appointmentId;
            ViewBag.ConsultationFee = doctor.ConsultationFee;
            ViewBag.DoctorName = doctor.FullName;

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PatientDetails(int doctorId, string doctorName, DateTime date,
                                                string selectedSlot, int isOnline, decimal consultationFee,
                                                int? appointmentId, AppointmentStatus? status)
        {
            ViewBag.DoctorId = doctorId;
            ViewBag.SelectedDate = date;
            ViewBag.SelectedSlot = selectedSlot;
            ViewBag.IsOnline = isOnline;
            ViewBag.AppointmentStatus = status ?? AppointmentStatus.Scheduled;
            ViewBag.AppointmentId = appointmentId;
            ViewBag.DoctorName = doctorName;
            ViewBag.ConsultationFee = consultationFee;

            if (appointmentId != null)
            {
                // Fetch existing appointment details for rescheduling
                var existingAppointment = await _appointmentService.GetAppointmentByIdAsync((int)appointmentId);
                if (existingAppointment != null)
                {
                    ViewBag.PatientName = existingAppointment.PatientName;
                    ViewBag.HealthConcern = existingAppointment.HealthConcern;
                }
            }

            return View();
        }



        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(
    int doctorId, string selectedSlot, DateTime date,
    int? clinicId, string patientName, string healthConcern,
    AppointmentStatus? status, int? appointmentId)
        {
            if (string.IsNullOrEmpty(selectedSlot))
            {
                TempData["Message"] = "Please select a valid time slot.";
                return RedirectToAction("BookAppointment", new { doctorId, date });
            }
            string consultationLink = string.Empty;
            bool isOnline = false;
            if (clinicId != null)
            {
                isOnline = true;
                consultationLink = $"https://meet.jit.si/healthconnect-room-{Guid.NewGuid()}#config.disableModerator=true";
            }

            if (appointmentId != null)
            {
                var userRole = User.IsInRole("Doctor") ? "Doctor" : "User";
                if (await _appointmentService.RescheduleAppointmentAsync((int)appointmentId, date, selectedSlot,clinicId, consultationLink,userRole))
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
                ClinicId = clinicId,
                ConsultationLink = consultationLink,
                Status = status ?? AppointmentStatus.Scheduled,
                PatientName = patientName,
                HealthConcern = healthConcern,
            };

            await _appointmentService.AddAppointmentAsync(newAppointment);

            // Schedule emails
            string userEmail = User.FindFirstValue(ClaimTypes.Email);
            string confirmationSubject = isOnline
                ? "Online Appointment Confirmation"
                : "In-Clinic Appointment Confirmation";
            string confirmationBody = isOnline
                ? $"Your appointment has been scheduled successfully. You will receive the meeting link 10 minutes before your scheduled time."
                : $"Your in-clinic appointment has been scheduled successfully for {date.ToShortDateString()} at {selectedSlot}.";

            _emailScheduler.ScheduleEmail(userEmail, confirmationSubject, confirmationBody, DateTime.Now.AddSeconds(1));
            if (TimeSpan.TryParse(selectedSlot, out var slotTime))
            {
                var appointmentDateTime = date.Date.Add(slotTime);

                if (isOnline)
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
            var userRole = User.IsInRole("Doctor") ? "Doctor" : "User";
            if (await _appointmentService.CancelAppointmentAsync(appointmentId,userRole))
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
