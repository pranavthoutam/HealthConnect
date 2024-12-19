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

        public IEnumerable<Doctor> ApplyFilters(IEnumerable<Doctor> doctors, DoctorFilterViewModel filter)
        {
            var query = doctors.AsQueryable();

            if (filter.Gender!=null)
            {
                query = query.Where(d => d.User.Gender == filter.Gender);
            }

            if (!string.IsNullOrEmpty(filter.Experience))
            {
                int minExperience = 0;
                if (filter.Experience == "5-10")
                    minExperience = 5;
                else if (filter.Experience == "10+")
                    minExperience = 10;

                query = query.Where(d => d.Experience >= minExperience);
            }

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

        public async Task<IEnumerable<string>> GetAvailableSlotsAsync(int doctorId, DateTime date, int clinicId)
        {
            var doctorAvailability = await _context.DoctorAvailability
                .Where(d => d.DoctorId == doctorId && d.ClinicId == clinicId)
                .FirstOrDefaultAsync();

            if (doctorAvailability == null)
                return Enumerable.Empty<string>();  

            var bookedSlots = (await GetAppointmentsForDoctorAsync(doctorId, date))
                .Select(a => a.Slot)
                .ToList();

            var availableSlots = doctorAvailability.AvailableSlots
                .Where(slot => !bookedSlots.Contains(slot))
                .ToList();

            if (date.Date == DateTime.Now.Date)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                availableSlots = availableSlots
                    .Where(slot => TimeSpan.TryParse(slot, out var slotTime) && slotTime > currentTime)
                    .ToList();
            }

            return availableSlots;
        }

        public async Task<IEnumerable<string>> GetAvailableOnlineSlotsAsync(int doctorId, DateTime date, int slotDuration)
        {
            var availability = await _context.DoctorAvailability
                                .Where(a => a.DoctorId == doctorId)
                                .ToListAsync();

            if (!availability.Any()) return Enumerable.Empty<string>();

            var minStartTime = availability.Min(a => a.StartTime);
            var maxEndTime = availability.Max(a => a.EndTime);

            var allSlots = new List<string>();
            for (var time = minStartTime; time < maxEndTime; time = time.Add(TimeSpan.FromMinutes(slotDuration)))
            {
                allSlots.Add(time.ToString(@"hh\:mm"));
            }

            var bookedSlots = await _context.Appointments
                                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                                .Select(a => a.Slot)
                                .ToListAsync();

            var currentTime = DateTime.Now.TimeOfDay;
            var availableSlots = allSlots
                                .Where(slot =>
                                    !bookedSlots.Contains(slot) &&
                                    (date.Date > DateTime.Now.Date || TimeSpan.Parse(slot) > currentTime))
                                .ToList();

            return availableSlots;
        }


        public async Task<IEnumerable<Appointment>> GetAppointmentsForUserAsync(string userId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
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

        public async Task<List<Clinic>> GetClinicsByDoctorIdAsync(int doctorId)
        {
            return await _context.Clinics
                .Where(c => c.DoctorId == doctorId)
                .Include(c => c.Availabilities)
                .ToListAsync();
        }

        public async Task<Clinic> GetClinicByIdAsync(int? clinicId)
        {
            return await _context.Clinics
                .Include(c => c.Availabilities)
                .FirstOrDefaultAsync(c => c.ClinicId == clinicId);
        }

        public async Task AddOrUpdateClinicAsync(Clinic clinic)
        {
            if (clinic.ClinicId == 0)
            {
                _context.Clinics.Add(clinic);
            }
            else
            {
                var existingClinic = await _context.Clinics.FindAsync(clinic.ClinicId);
                if (existingClinic != null)
                {
                    existingClinic.ClinicName = clinic.ClinicName;
                    existingClinic.HnoAndStreetName = clinic.HnoAndStreetName;
                    existingClinic.District = clinic.District;
                    existingClinic.Place = clinic.Place;
                    _context.Clinics.Update(existingClinic);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteClinicAsync(int clinicId)
        {
            var clinic = await GetClinicByIdAsync(clinicId);
            if (clinic != null)
            {
                _context.Clinics.Remove(clinic);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAvailabilityAsync(int doctorId, int clinicId, TimeSpan startTime, TimeSpan endTime, int slotDuration)
        {
            var doctorAvailability = await _context.DoctorAvailability
                .Where(d => d.DoctorId == doctorId)
                .ToListAsync();

            foreach (var availability in doctorAvailability)
            {
                if ((startTime >= availability.StartTime && startTime < availability.EndTime) ||
                    (endTime > availability.StartTime && endTime <= availability.EndTime))
                {
                    throw new Exception("The selected time slot overlaps with an existing availability for this doctor in another clinic.");
                }
            }

            var newAvailability = new DoctorAvailability
            {
                ClinicId = clinicId,
                DoctorId = doctorId,
                StartTime = startTime,
                EndTime = endTime,
                SlotDuration = slotDuration
            };

            _context.Add(newAvailability);
            await _context.SaveChangesAsync();
        }
    }
}
