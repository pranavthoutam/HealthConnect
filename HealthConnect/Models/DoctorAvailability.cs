namespace HealthConnect.Models
{
    public class DoctorAvailability
    {
        public int DoctorAvailabilityId { get; set; }
        public int ClinicId { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public virtual Clinic Clinic { get; set; }
    }
}
