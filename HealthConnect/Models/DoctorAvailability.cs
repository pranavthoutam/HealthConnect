namespace HealthConnect.Models
{
    public class DoctorAvailability
    {
        public int DoctorAvailabilityId { get; set; }
        public int DoctorId { get; set; }

        public int ClinicId { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Range(15,60)]//In Minutes
        public int SlotDuration { get; set; }

        public Doctor Doctor { get; set; }
        public virtual Clinic Clinic { get; set; }

        public List<string> AvailableSlots => GenerateAvailableSlots();

        public List<string> GenerateAvailableSlots()
        {
            var slots = new List<string>();
            var current = StartTime;

            while (current < EndTime)
            {
                slots.Add(current.ToString(@"hh\:mm"));
                current = current.Add(TimeSpan.FromMinutes(SlotDuration));
            }

            return slots;
        }
    }
}
