﻿namespace HealthConnect.Services
{
    public class AppointmentService
    {
        private readonly DoctorRepository _doctorRepository;
        public AppointmentService(DoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _doctorRepository.GetAppointmentByIdAsync(appointmentId);
        }

        public async Task<bool> RescheduleAppointmentAsync(int appointmentId, DateTime date, string slot, int? clinicId, string consultationLink,string userRole)
        {
            var appointment = await _doctorRepository.GetAppointmentByIdAsync(appointmentId);
            var clinic = await _doctorRepository.GetClinicByIdAsync(clinicId);
            if (appointment != null)
            {
                // Check if rescheduling is allowed (at least 3 hours before the appointment)
                if (userRole!="Doctor" && (appointment.AppointmentDate - DateTime.Now).TotalHours > 3)
                {
                    return false; // Cannot reschedule within 3 hours of the appointment
                }

                // Reschedule the appointment
                appointment.AppointmentDate = date;
                appointment.Slot = slot;
                appointment.Place=clinic.Place;
                appointment.ClinicName = clinic.ClinicName;
                appointment.ConsultationLink = consultationLink;
                appointment.Status = AppointmentStatus.ReScheduled;
                appointment.ClinicId = clinicId;
                await _doctorRepository.RescheduleAppointment(appointment);
                return true;
            }
            return false;
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, string userRole)
        {
            var appointment = await _doctorRepository.GetAppointmentByIdAsync(appointmentId);
            if (appointment != null)
            {
                // Check if cancellation is allowed (at least 3 hours before the appointment)
                if (userRole!="Doctor" && (appointment.AppointmentDate - DateTime.Now).TotalHours < 3)
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
