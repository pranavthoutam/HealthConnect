namespace HealthConnect.Services
{
    public class DoctorService
    {
        private readonly DoctorRepository _doctorRepository;

        public DoctorService(DoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public IEnumerable<Doctor> FindDoctors(string location, string searchString, string loggedInUserId = null)
        {
            return _doctorRepository.GetDoctorsByLocationAndSpecialization(location, searchString, loggedInUserId);
        }

        public IEnumerable<Doctor> ApplyFilters(IEnumerable<Doctor> doctors, DoctorFilterViewModel filter)
        {
            return _doctorRepository.ApplyFilters(doctors, filter);
        }

        public async Task<Doctor> GetDoctorProfileAsync(int doctorId)
        {
            return await _doctorRepository.SearchDoctorAsync(doctorId);
        }

        public async Task<IEnumerable<string>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            return await _doctorRepository.GetAvailableSlotsAsync(doctorId, date);
        }
    }
}
