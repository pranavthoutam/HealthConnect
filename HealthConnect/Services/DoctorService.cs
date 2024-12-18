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

        public async Task<IEnumerable<string>> GetAvailableSlotsAsync(int doctorId, DateTime date, int selectedClinicId)
        {
            return await _doctorRepository.GetAvailableSlotsAsync(doctorId, date, selectedClinicId);
        }

        public async Task<IEnumerable<string>> GetAvailableOnlineSlotsAsync(int doctorId, DateTime date)
        {
            const int slotDuration = 15;
            return await _doctorRepository.GetAvailableOnlineSlotsAsync(doctorId, date, slotDuration);
        }

    }
}
