namespace HealthConnect.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class ClinicController : Controller
    {
        private readonly ClinicService _clinicService;
        private readonly DoctorRepository _doctorRepository;

        public ClinicController(ClinicService clinicService, DoctorRepository doctorRepository)
        {
            _clinicService = clinicService;
            _doctorRepository = doctorRepository;
        }

        [HttpGet]
        public async Task<IActionResult> ClinicDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            int doctorId = _doctorRepository.GetDoctorId(userId);
            var clinics = await _clinicService.GetClinicsAsync(doctorId);

            ViewBag.DoctorId = doctorId;
            return View(clinics);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateClinic(Clinic clinic, IFormFile ClinicImage)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid clinic details.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int doctorId = _doctorRepository.GetDoctorId(userId);

            await _clinicService.AddOrUpdateClinicAsync(clinic, ClinicImage, doctorId);

            return RedirectToAction(nameof(ClinicDashboard));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteClinic(int clinicId)
        {
            await _clinicService.DeleteClinicAsync(clinicId);
            return RedirectToAction(nameof(ClinicDashboard));
        }

        public async Task<IActionResult> UpdateAvailability(int clinicId, TimeSpan? startTime, TimeSpan? endTime, int? slotDuration)
        {
            if (startTime.HasValue && endTime.HasValue && slotDuration.HasValue)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int doctorId = _doctorRepository.GetDoctorId(userId);

                try
                {
                    await _clinicService.AddAvailabilityAsync(doctorId, clinicId, startTime.Value, endTime.Value, slotDuration.Value);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("StartTime", ex.Message);
                }
            }

            return RedirectToAction(nameof(ClinicDashboard));
        }
    }
}
