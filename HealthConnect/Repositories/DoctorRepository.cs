using HealthConnect.Data;
using HealthConnect.Models;
using HealthConnect.ViewModels;
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

        // Fetch doctors by location and specialization
        public IEnumerable<Doctor> GetDoctorsByLocationAndSpecialization(string location, string searchString)
        {
            var query = _context.Doctors.Include(d=>d.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(d => d.Place.Contains(location));
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d => d.Specialization.Contains(searchString) || d.FullName.Contains(searchString) );
            }

            return query.ToList();
        }

        // Apply filters such as gender, experience, and sorting
        public IEnumerable<Doctor> ApplyFilters(IEnumerable<Doctor> doctors, DoctorFilterViewModel filter)
        {
            var query = doctors.AsQueryable();

            // Apply Gender Filter
            if (filter.Gender!=null)
            {
                query = query.Where(d => d.User.Gender == filter.Gender);
                //query = query.Where(d => d.User != null && d.User.Gender == filter.Gender);
            }

            // Apply Experience Filter
            if (!string.IsNullOrEmpty(filter.Experience))
            {
                int minExperience = 0;
                if (filter.Experience == "5-10")
                    minExperience = 5;
                else if (filter.Experience == "10+")
                    minExperience = 10;

                query = query.Where(d => d.Experience >= minExperience);
            }

            // Sorting logic based on the selected criteria
            switch (filter.SortBy)
            {
                case "Rating":
                    query = query.OrderByDescending(d => d.Rating);
                    break;
                case "Experience":
                    query = query.OrderByDescending(d => d.Experience);
                    break;
                case "Fees":
                    query = query.OrderBy(d => d.ConsultationFee);
                    break;
            }

            return query.ToList();
        }

        public async Task<Doctor> SearchDoctorAsync(int doctorId)
        {
            return await _context.Doctors
                         .Include(d => d.User) // Ensure User is included
                         .FirstOrDefaultAsync(d => d.Id == doctorId);
        }


        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsForDoctorAsync(int doctorId, DateTime date)
        {
            return await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate == date)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor == null) return Enumerable.Empty<string>();

            var bookedSlots = (await GetAppointmentsForDoctorAsync(doctorId, date)).Select(a => a.Slot).ToList();
            var availableSlots = doctor.AvailableSlots.Where(s => !bookedSlots.Contains(s));
            return availableSlots;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsForUserAsync(string userId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor) // Include related Doctor
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
        }


    }
}
