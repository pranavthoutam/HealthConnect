namespace HealthConnect.Services
{
    public class ClinicService
    {
        private readonly DoctorRepository _doctorRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ClinicService(DoctorRepository doctorRepository, IWebHostEnvironment webHostEnvironment)
        {
            _doctorRepository = doctorRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<Clinic>> GetClinicsAsync(int doctorId)
        {
            return await _doctorRepository.GetClinicsByDoctorIdAsync(doctorId);
        }

        public async Task AddOrUpdateClinicAsync(Clinic clinic, IFormFile clinicImage, int doctorId)
        {
            if (clinicImage != null && clinicImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "ClinicImages");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(clinicImage.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await clinicImage.CopyToAsync(fileStream);
                }

                clinic.ClinicImagePath = $"/Uploads/ClinicImages/{uniqueFileName}";
            }

            await _doctorRepository.AddOrUpdateClinicAsync(clinic);
        }

        public async Task DeleteClinicAsync(int clinicId)
        {
            await _doctorRepository.DeleteClinicAsync(clinicId);
        }

        public async Task AddAvailabilityAsync(int doctorId, int clinicId, TimeSpan startTime, TimeSpan endTime, int slotDuration)
        {
            await _doctorRepository.AddAvailabilityAsync(doctorId, clinicId, startTime, endTime, slotDuration);
        }
    }
}
