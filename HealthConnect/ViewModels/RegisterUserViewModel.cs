using System.ComponentModel.DataAnnotations;

namespace HealthConnect.ViewModels
{
    public class RegisterUserViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public bool Role {  get; set; } = false;
    }
}
