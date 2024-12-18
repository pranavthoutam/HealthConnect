namespace HealthConnect.Models
{
    public class Clinic
    {
        public int ClinicId { get; set; }

        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Clinic name is required.")]
        [StringLength(100, ErrorMessage = "Clinic name can't be longer than 100 characters.")]
        public string ClinicName { get; set; }

        [StringLength(200, ErrorMessage = "Street address can't be longer than 200 characters.")]
        public string HnoAndStreetName { get; set; }

        [StringLength(100, ErrorMessage = "District name can't be longer than 100 characters.")]
        public string District { get; set; }

        [StringLength(100, ErrorMessage = "Place name can't be longer than 100 characters.")]
        public string Place { get; set; }

        [Url(ErrorMessage = "Invalid image URL.")]
        public string? ClinicImagePath { get; set; }

        public virtual Doctor? Doctor { get; set; }
        public virtual ICollection<DoctorAvailability>? Availabilities { get; set; }
    }
}
