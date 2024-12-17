
namespace HealthConnect.Repositories
{
    public class DoctorRepository 
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
        public IEnumerable<Doctor> GetDoctorsByLocationAndSpecialization(string location, string searchString, string loggedInUserId = null)
        {

            var query = _context.Doctors
                    .Include(d=>d.User)
                    .Include(d=>d.Clinics)
                    .Where(d=>d.ApprovalStatus=="Approved" && d.UserId != loggedInUserId )
                .AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(d => d.Clinics.Any(c => c.Place.Contains(location)));
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d => d.Specialization.Contains(searchString) || EF.Functions.Like(d.FullName, $"%{searchString}%") );
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
                         .Include(d => d.User)
                         .Include(d=>d.Clinics)
                         .FirstOrDefaultAsync(d => d.Id == doctorId);
        }


        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task RescheduleAppointment(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task CancelAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Remove(appointment);
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
            if (date.Date == DateTime.Now.Date)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                availableSlots = availableSlots
                    .Where(slot =>
                    {
                        if (TimeSpan.TryParse(slot, out var slotTime))
                        {
                            return slotTime > currentTime;
                        }
                        return false;
                    })
                    .ToList();
            }
            return availableSlots;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsForUserAsync(string userId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
        }
        public async Task<Appointment> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
        }
        
        public async Task<IEnumerable<Feedback>> GetFeedBacksAsync(string userId)
        {
            return await _context.Feedbacks.Include(f => f.Appointment).Where(f=>f.UserId == userId).ToListAsync();
        }

        public async Task<Appointment> GetAppointmentByDetailsAsync(int doctorId, string userId, DateTime date, string slot)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.DoctorId == doctorId &&
                    a.UserId == userId &&
                    a.AppointmentDate.Date == date.Date &&
                    a.Slot == slot);
        }

        public int GetDoctorId(string userId)
        {
            return  _context.Doctors.FirstOrDefault(d=>d.UserId==userId).Id;
        }
    }
}
