namespace HealthConnect.Services
{
    public class AppointmentService
    {
        private readonly DoctorRepository _doctorRepository;
        public AppointmentService(DoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<bool> RescheduleAppointmentAsync(int appointmentId, DateTime date, string slot, bool isOnline, string consultationLink)
        {
            var appointment = await _doctorRepository.GetAppointmentByIdAsync(appointmentId);
            if (appointment != null)
            {
                // Check if rescheduling is allowed (at least 3 hours before the appointment)
                if ((appointment.AppointmentDate - DateTime.Now).TotalHours < 3)
                {
                    return false; // Cannot reschedule within 3 hours of the appointment
                }

                // Reschedule the appointment
                appointment.AppointmentDate = date;
                appointment.Slot = slot;
                appointment.IsOnline = isOnline;
                appointment.ConsultationLink = consultationLink;
                appointment.Status = AppointmentStatus.ReScheduled;

                await _doctorRepository.RescheduleAppointment(appointment);
                return true;
            }
            return false;
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            var appointment = await _doctorRepository.GetAppointmentByIdAsync(appointmentId);
            if (appointment != null)
            {
                // Check if cancellation is allowed (at least 3 hours before the appointment)
                if ((appointment.AppointmentDate - DateTime.Now).TotalHours < 3)
                {
                    return false; // Cannot cancel within 3 hours of the appointment
                }

                // Cancel the appointment
                appointment.Status = AppointmentStatus.Canceled;
                await _doctorRepository.CancelAppointmentAsync(appointment);
                return true;
            }
            return false;
        }

        public async Task<bool> IsDuplicateAppointmentAsync(int doctorId, string userId, DateTime date, string slot)
        {
            var appointment = await _doctorRepository.GetAppointmentByDetailsAsync(doctorId, userId, date, slot);
            return appointment != null;
        }

        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await _doctorRepository.AddAppointmentAsync(appointment);
        }
    }
}
