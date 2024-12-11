public class EmailScheduler
{
    private readonly EmailService _emailService;

    public EmailScheduler(EmailService emailService)
    {
        _emailService = emailService;
    }

    public void ScheduleEmail(
        string toEmail,
        string subject,
        string body,
        DateTime scheduledTime)
    {
        var delay = scheduledTime - DateTime.Now;

        if (delay <= TimeSpan.Zero)
        {
            // If the scheduled time is in the past, send the email immediately.
            _ = _emailService.SendEmailAsync(toEmail, subject, body);
            return;
        }

        // Use a timer to schedule the email.
        var timer = new Timer(async _ =>
        {
            // Send the email.
            await _emailService.SendEmailAsync(toEmail, subject, body);

            // Dispose of the timer after the email is sent.
            ((Timer)_).Dispose();
        }, null, delay, Timeout.InfiniteTimeSpan);
    }
}
