using HealthConnect.Models;
using HealthConnect.ViewModels;
namespace HealthConnect.Repositories
{
    public interface IDoctorRepository
    {
        Task AddDoctorAsync(Doctor doctor);
        IEnumerable<Doctor> GetDoctorsByLocationAndSpecialization(string location, string searchString);
        IEnumerable<Doctor> ApplyFilters(IEnumerable<Doctor> doctors, DoctorFilterViewModel filter);

        Task<Doctor> SearchDoctorAsync(int doctorId);

        Task<IEnumerable<string>> GetAvailableSlotsAsync(int doctorId, DateTime date);
        Task AddAppointmentAsync(Appointment appointment);
        Task<IEnumerable<Appointment>> GetAppointmentsForUserAsync(string userId);

        Task<Appointment> GetAppointmentByIdAsync(int appointmentId);
        Task RescheduleAppointment(Appointment appointment);
        Task CancelAppointmentAsync(Appointment appointment);
    }
}
