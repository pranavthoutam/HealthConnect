namespace HealthConnect.ViewModels
{
    public class BookDoctorAppointmentViewModel
    {
        public Doctor Doctor { get; set; }
        public int? SelectedClinicId { get; set; }
        public IEnumerable<Clinic> Clinics { get; set; }
        public Clinic SelectedClinic { get; set; }
    }

}
