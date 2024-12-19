namespace HealthConnect.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; } 
        public byte[] PrescriptionPdf { get; set; } 
        public Appointment Appointment { get; set; }
    }

}
