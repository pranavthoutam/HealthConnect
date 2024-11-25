using HealthConnect.Models;

namespace HealthConnect.ViewModels
{
    public class DoctorFilterViewModel
    {
        public string? Location { get; set; }
        public string? SearchString { get; set; }
        public Gender? Gender { get; set; }
        public string? Experience { get; set; }
        public string? SortBy { get; set; }

        // List of doctors that will be displayed after filters
        public List<Doctor>? Doctors { get; set; }
    }

}
