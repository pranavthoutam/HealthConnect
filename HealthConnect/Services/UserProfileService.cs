namespace HealthConnect.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<User> _userManager;
        private readonly DoctorRepository _doctorRepository;

        public UserProfileService(UserManager<User> userManager,DoctorRepository doctorRepository)
        {
            _userManager = userManager;
            _doctorRepository = doctorRepository;
        }

        public async Task<ProfileDashboardViewModel> GetDashboardAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            IEnumerable<Appointment> appointments = await _doctorRepository.GetAppointmentsForUserAsync(userId);

            List<Feedback> feedbacks = (List<Feedback>)await _doctorRepository.GetFeedBacksAsync(userId);

            // Process appointments
            DateTime currentTime = DateTime.Now;
            ProfileDashboardViewModel viewModel = new ProfileDashboardViewModel
            {
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePhoto = user.ProfilePhoto,
                Name = user.UserName,
                Feedbacks = feedbacks,
                InClinicAppointments = appointments
                                       .Where(a => a.IsOnline == false)
                                       .Select(a => new AppointmentViewModel
                                       {
                                           AppointmentId = a.Id,
                                           DoctorName = a.Doctor.FullName,
                                           DoctorId = a.Doctor.Id,
                                           DoctorSpecialization = a.Doctor.Specialization,
                                           AppointmentDate = a.AppointmentDate,
                                           Slot = a.Slot,
                                           Location = a.Doctor.Place,
                                           CanRescheduleOrCancel = a.AppointmentDate.Add(TimeSpan.Parse(a.Slot)).Subtract(currentTime).TotalHours > 3,
                                           IsCompleted = a.AppointmentDate.Date < currentTime.Date ||
                                                        (a.AppointmentDate.Date == currentTime.Date && TimeSpan.Parse(a.Slot) < currentTime.TimeOfDay),
                                       })
                                        .ToList(),


                OnlineConsultations = appointments
                                        .Where(a => a.IsOnline == true)
                                        .Select(a => new AppointmentViewModel
                                        {
                                            AppointmentId = a.Id,
                                            DoctorName = a.Doctor.FullName,
                                            DoctorId = a.Doctor.Id,
                                            DoctorSpecialization = a.Doctor.Specialization,
                                            AppointmentDate = a.AppointmentDate,
                                            Slot = a.Slot,
                                            Location = "Online",
                                            CanRescheduleOrCancel = a.AppointmentDate.Add(TimeSpan.Parse(a.Slot)).Subtract(currentTime).TotalHours > 3,
                                            IsCompleted = a.AppointmentDate.Date < currentTime.Date ||
                                                            (a.AppointmentDate.Date == currentTime.Date && TimeSpan.Parse(a.Slot) < currentTime.TimeOfDay),
                                            MeetingLink = a.ConsultationLink
                                        })
                                        .ToList(),

                CompletedAppointments = appointments
                                        .Where(a => a.AppointmentDate.Date < currentTime.Date ||
                                            (a.AppointmentDate.Date == currentTime.Date && TimeSpan.Parse(a.Slot) <= currentTime.TimeOfDay))
                                        .Select(a => new AppointmentViewModel
                                        {
                                            AppointmentId = a.Id,
                                            DoctorName = a.Doctor.FullName,
                                            DoctorId = a.Doctor.Id,
                                            DoctorSpecialization = a.Doctor.Specialization,
                                            AppointmentDate = a.AppointmentDate,
                                            Slot = a.Slot,
                                            Location = a.Doctor.Place ?? "Online"
                                        })
                                        .ToList()
            };

            return viewModel;


        }

        public async Task<UserProfileViewModel> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                BloodGroup = user.BloodGroup,
                DateofBirth = user.DateofBirth,
                HouseNumber = user.HouseNumber,
                Street = user.Street,
                City = user.City,
                State = user.State,
                Country = user.Country,
                PostalCode = user.PostalCode,
            };
        }

        public async Task<IdentityResult> UpdateUserProfileAsync(string userId, UserProfileViewModel updatedUser)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            user.UserName = updatedUser.UserName;
            user.DateofBirth = updatedUser.DateofBirth;
            user.Gender = updatedUser.Gender;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.BloodGroup = updatedUser.BloodGroup;
            user.HouseNumber = updatedUser.HouseNumber;
            user.Street = updatedUser.Street;
            user.City = updatedUser.City;
            user.State = updatedUser.State;
            user.Country = updatedUser.Country;
            user.PostalCode = updatedUser.PostalCode;

            if (updatedUser.ProfilePhoto != null && updatedUser.ProfilePhoto.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await updatedUser.ProfilePhoto.CopyToAsync(memoryStream);
                    user.ProfilePhoto = memoryStream.ToArray();
                }
            }

            return await _userManager.UpdateAsync(user);
        }

        public async Task<byte[]> GetProfilePhotoAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.ProfilePhoto;
        }
    }
}
