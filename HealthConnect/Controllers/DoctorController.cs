namespace HealthConnect.Controllers
{
    public class DoctorController : Controller
    {
        private readonly DoctorRepository _doctorRepository;
        public DoctorController(DoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public IActionResult Register()
        {
            var model = new DoctorRegistrationViewModel();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Register(DoctorRegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                string clinicImagePath = null;
                string certificatePath = null;

                if (model.ClinicImage != null && model.ClinicImage.Length > 0)
                {
                    clinicImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "clinicimages", model.ClinicImage.FileName);
                    using (var stream = new FileStream(clinicImagePath, FileMode.Create))
                    {
                        await model.ClinicImage.CopyToAsync(stream);
                    }
                }
                if (model.Certificate != null && model.Certificate.Length > 0)
                {
                    certificatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "certificates", model.Certificate.FileName);
                    using (var stream = new FileStream(certificatePath, FileMode.Create))
                    {
                        await model.Certificate.CopyToAsync(stream);
                    }
                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var doctor = new Doctor
                {
                    FullName = model.FullName,
                    Specialization = model.Specialization,
                    MciRegistrationNumber = model.MciRegistrationNumber,
                    Experience = model.Experience,
                    ConsultationFee = model.ConsultationFee,
                    OnlineConsultation = model.OnlineConsultation,
                    ClinicAppointment = model.ClinicAppoinment,
                    ClinicName = model.ClinicName,
                    ClinicImagePath = "/uploads/clinicimages/" + model.ClinicImage.FileName,
                    CertificatePath = "/uploads/certificates/" + model.Certificate.FileName,
                    ApprovalStatus = "Pending" ,
                    HnoAndStreetName = model.HnoAndStreetName,
                    Place=model.Place,
                    District=model.District,
                    UserId=userId

                };

                await _doctorRepository.AddDoctorAsync(doctor);
                return RedirectToAction("Index", "Doctor");
            }
            return View(model);
        }
        public async Task<IActionResult> TodaysAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var doctorId = _doctorRepository.GetDoctorId(userId);

            var appointments = (List<Appointment>) await _doctorRepository.GetAppointmentsForDoctorAsync(doctorId,DateTime.Now.Date);

            var viewModel = new ProfileDashboardViewModel
            {
               Appointments = appointments
            };

            return View(viewModel); 
        }
    }
}
