namespace HealthConnect.Models
{
    public class Clinic
    {
        public int ClinicId { get; set; }
        public int DoctorId { get; set; }

        [Required]
        public string ClinicName { get; set; }

        public string HnoAndStreetName { get; set; }
        public string District { get; set; }
        public string Place { get; set; }
        public string? ClinicImagePath { get; set; }
        public virtual Doctor? Doctor { get; set; }
        public virtual ICollection<DoctorAvailability>? Availabilities { get; set; }
    }
}
