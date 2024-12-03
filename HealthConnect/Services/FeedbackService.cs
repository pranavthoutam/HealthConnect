using HealthConnect.Data;
using HealthConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthConnect.Services
{
    public class FeedbackService
    {
        private readonly ApplicationDbContext _context;

        public FeedbackService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SubmitFeedbackAsync(Feedback feedback)
        {
            // Save feedback to the database
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            // Update doctor's average rating
            await UpdateDoctorRatingAsync(feedback.DoctorId);
        }

        public async Task<double> CalculateAverageRatingAsync(int doctorId)
        {
            var ratings = await _context.Feedbacks
        .Where(f => f.DoctorId == doctorId)
        .Select(f => (double?)f.Rating) // Use nullable to handle empty collections
        .ToListAsync();

            return ratings.Any() ? ratings.Average() ?? 0 : 0;

        }

        private async Task UpdateDoctorRatingAsync(int doctorId)
        {
            var averageRating = await CalculateAverageRatingAsync(doctorId);

            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor != null)
            {
                doctor.Rating = averageRating;
                await _context.SaveChangesAsync();
            }
        }
    }

}
