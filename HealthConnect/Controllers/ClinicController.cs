namespace HealthConnect.Controllers
{
    [Authorize]
    public class ClinicController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DoctorRepository _doctorRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ClinicController(ApplicationDbContext context, DoctorRepository doctorRepository, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _doctorRepository = doctorRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> ClinicDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            int doctorId = _doctorRepository.GetDoctorId(userId);
            var clinics = await _context.Clinics
                .Where(c => c.DoctorId == doctorId)
                .Include(c => c.Availabilities)
                .ToListAsync();
            if(doctorId!=null) ViewBag.DoctorId=doctorId;
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

            if (ClinicImage != null)
            {
                // Save image to wwwroot/Uploads/ClinicImages
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "ClinicImages");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ClinicImage.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ClinicImage.CopyToAsync(fileStream);
                }

                clinic.ClinicImagePath = $"/Uploads/ClinicImages/{uniqueFileName}";
            }

            if (clinic.ClinicId == 0)
            {
                // Add new clinic
                clinic.DoctorId = doctorId;
                _context.Clinics.Add(clinic);
            }
            else
            {
                // Update existing clinic
                var existingClinic = await _context.Clinics.FindAsync(clinic.ClinicId);
                if (existingClinic == null)
                    return NotFound("Clinic not found.");

                existingClinic.ClinicName = clinic.ClinicName;
                existingClinic.HnoAndStreetName = clinic.HnoAndStreetName;
                existingClinic.District = clinic.District;
                existingClinic.Place = clinic.Place;

                if (ClinicImage != null)
                {
                    // Delete old image file if it exists
                    if (!string.IsNullOrEmpty(existingClinic.ClinicImagePath))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingClinic.ClinicImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    existingClinic.ClinicImagePath = clinic.ClinicImagePath;
                }

                _context.Clinics.Update(existingClinic);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ClinicDashboard));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteClinic(int clinicId)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Availabilities)
                .FirstOrDefaultAsync(c => c.ClinicId == clinicId);

            if (clinic == null)
                return NotFound("Clinic not found.");

            // Delete clinic image from wwwroot if it exists
            if (!string.IsNullOrEmpty(clinic.ClinicImagePath))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, clinic.ClinicImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Clinics.Remove(clinic);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ClinicDashboard));
        }

        [HttpGet]
        public async Task<IActionResult> EditAvailability(int clinicId)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Availabilities)
                .FirstOrDefaultAsync(c => c.ClinicId == clinicId);

            if (clinic == null)
                return NotFound("Clinic not found.");

            return View(clinic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAvailability(int clinicId, List<DoctorAvailability> availabilities)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Availabilities)
                .FirstOrDefaultAsync(c => c.ClinicId == clinicId);

            if (clinic == null)
                return NotFound("Clinic not found.");

            // Remove old availabilities
            _context.DoctorAvailability.RemoveRange(clinic.Availabilities);

            // Add new availabilities
            clinic.Availabilities = availabilities;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ClinicDashboard));
        }
    }
}
