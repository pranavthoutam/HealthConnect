
namespace HealthConnect.ViewModels
{
    public class ProfileDashboardViewModel
    {
        public List<AppointmentViewModel>? InClinicAppointments { get; set; }
        public List<AppointmentViewModel>? OnlineConsultations { get; set; }
        public List<AppointmentViewModel>? CompletedAppointments { get; set; }
        public List<Feedback>? Feedbacks { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; } 

        public Byte[] ProfilePhoto { get; set; }

        public string Name { get; set; }

    }

    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public string DoctorName { get; set; }
        public int DoctorId { get; set; }
        public string DoctorSpecialization { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Slot { get; set; }
        public string Location { get; set; }
        public bool CanRescheduleOrCancel { get; set; }
        public bool IsCompleted { get; set; } = false;

        public string MeetingLink { get; set; }
    }

}
