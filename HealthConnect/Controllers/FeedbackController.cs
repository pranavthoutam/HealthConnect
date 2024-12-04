public class FeedbackController : Controller
{
    private readonly FeedbackService _feedbackService;

    public FeedbackController(FeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpGet]
    public IActionResult SubmitFeedback(int doctorId, string userId, int appointmentId)
    {
        var feedback = new Feedback
        {
            DoctorId = doctorId,
            UserId = userId,
            AppointmentId = appointmentId
        };
        return View(feedback); // Returns the feedback form view
    }

    [HttpPost]
    public async Task<IActionResult> SubmitFeedback(Feedback feedback)
    {
        ModelState.Remove("Doctor");
        ModelState.Remove("User");
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please provide valid feedback.";
            return View(feedback);
        }

        try
        {
            await _feedbackService.SubmitFeedbackAsync(feedback);
            TempData["SuccessMessage"] = "Thank you for your feedback!";
            return RedirectToAction("ProfileDashboard", "User");
        }
        catch (Exception ex)
        {
            // Log error if needed
            TempData["ErrorMessage"] = "An error occurred while submitting feedback.";
            return View(feedback);
        }
    }
}
