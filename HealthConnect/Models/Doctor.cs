using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthConnect.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        [Required]
        public string Specialization { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal ConsultationFee { get; set; }

        public float Rating { get; set; }

        public string ApprovalStatus { get; set; }

        [Required]
        public string MciRegistrationNumber { get; set; }

        public string FullName { get; set; }

        [Required]
        public int Experience { get; set; }

        public bool OnlineConsultation { get; set; }

        public bool ClinicAppointment { get; set; }

        public string ClinicName { get; set; }

        public string HnoAndStreetName { get; set; }

        public string District { get; set; }

        public string Place { get; set; }

        // Store file paths instead of byte arrays
        public string ClinicImagePath { get; set; }

        public string CertificatePath { get; set; }

        public virtual User User { get; set; }

        //Slot Management

        
        [NotMapped]
        public List<string> AvailableSlots => GenerateAvailableSlots();

        private List<string> GenerateAvailableSlots()
        {
            var slots = new List<string>();
            var timeRanges = new[]
            {
                (Start: TimeSpan.Parse("10:00"), End: TimeSpan.Parse("13:00")),
                (Start: TimeSpan.Parse("14:00"), End: TimeSpan.Parse("16:00")),
                (Start: TimeSpan.Parse("18:00"), End: TimeSpan.Parse("20:00"))
            };

            foreach (var range in timeRanges)
            {
                var current = range.Start;
                while (current < range.End)
                {
                    slots.Add(current.ToString(@"hh\:mm"));
                    current = current.Add(TimeSpan.FromMinutes(15));
                }
            }
            return slots;
        }
    }
}
