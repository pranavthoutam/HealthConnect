namespace HealthConnect.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int DoctorId { get; set; }

        public int rating { get; set; }
        
        public string Description { get; set; }
    }
}
