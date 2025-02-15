﻿namespace HealthConnect.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public string PatientName { get; set; }

        [ForeignKey("ClinicId")]
        public int? ClinicId { get; set; }
        [Required]
        public string HealthConcern { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string Slot { get; set; }

        //Nullable ClinicId to account for online consultations
        public string? ClinicName    { get; set; }
        public string? Place {  get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        public string? ConsultationLink { get; set; }
    }

    public enum AppointmentStatus
    {
        Scheduled,
        ReScheduled,
        Canceled
    }
}
