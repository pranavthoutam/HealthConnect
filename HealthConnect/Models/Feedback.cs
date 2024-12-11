
namespace HealthConnect.Models
{
    public class Feedback
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
        [Range(1,5,ErrorMessage ="Rating is Required")]
        public int Rating { get; set; }
        [Required]
        public string Description { get; set; }

        [ForeignKey("AppointmentId")]
        [Required]
        public int? AppointmentId { get; set; }
        
        public Appointment Appointment { get; set; }

        [Required]
        public DateTime FeedbackDate { get; set; }
    }
}
