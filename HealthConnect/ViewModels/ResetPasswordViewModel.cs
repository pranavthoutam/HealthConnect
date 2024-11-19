using System.ComponentModel.DataAnnotations;

namespace HealthConnect.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }


}
