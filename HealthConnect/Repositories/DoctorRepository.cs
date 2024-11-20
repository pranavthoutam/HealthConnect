using HealthConnect.Data;
using HealthConnect.Models;
using Microsoft.EntityFrameworkCore;


namespace HealthConnect.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddDoctorAsync(Doctor doctor)
        {
            await _context.Doctors.AddAsync(doctor);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Doctor>> SearchDoctorsAsync(string searchString)
        {
            return await _context.Doctors
                .Where(d => d.FullName.Contains(searchString) || d.Specialization.Contains(searchString))
                .ToListAsync();
        }
    }
}
