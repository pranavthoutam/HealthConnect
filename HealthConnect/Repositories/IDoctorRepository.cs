using HealthConnect.Models;
namespace HealthConnect.Repositories
{
    public interface IDoctorRepository
    {
        Task AddDoctorAsync(Doctor doctor);
        Task<IEnumerable<Doctor>> SearchDoctorsAsync(string searchString);
    }
}
