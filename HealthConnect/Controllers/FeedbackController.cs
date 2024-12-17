public class FeedbackController : Controller
{
    private readonly FeedbackService _feedbackService;

    public FeedbackController(FeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpGet]
    public async Task<IActionResult> SubmitFeedback(int appointmentId)
    {
        var appointment =await  _feedbackService.GetAppointmentByIdAsync(appointmentId);

        if (appointment == null)
        {
            return NotFound(); // Handle invalid appointment case
        }
        var feedback = new Feedback
        {
            DoctorId = appointment.DoctorId,
            UserId = appointment.UserId,
            AppointmentId = appointmentId,
            Appointment = appointment,
            Doctor = appointment.Doctor,
            User = appointment.User
        };
        return View(feedback);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitFeedback(Feedback feedback)
    {
        feedback.FeedbackDate = DateTime.Now;
        ModelState.Remove("Doctor");
        ModelState.Remove("User");
        ModelState.Remove("Appointment");
        if (!ModelState.IsValid)
        {
            return View(feedback);
        }

        try
        {
            await _feedbackService.SubmitFeedbackAsync(feedback);
            return RedirectToAction("FeedbackConfirmation");
        }
        catch (Exception ex)
        {
            return View(feedback);
        }
    }

    public IActionResult FeedbackConfirmation()
    {
        return View();
    }
}
