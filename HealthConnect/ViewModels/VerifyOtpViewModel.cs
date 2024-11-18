using System.ComponentModel.DataAnnotations;

namespace HealthConnect.ViewModels
{
    public class VerifyOtpViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string OTP { get; set; }
    }

}
