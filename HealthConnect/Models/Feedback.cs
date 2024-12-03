using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public int Rating { get; set; }

        public string Description { get; set; }

        [ForeignKey("AppointmentId")]
        [Required]
        public int? AppointmentId { get; set; }
        
        public Appointment Appointment { get; set; }
    }
}
