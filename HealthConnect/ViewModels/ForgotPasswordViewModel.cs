using System.ComponentModel.DataAnnotations;

namespace HealthConnect.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

}
