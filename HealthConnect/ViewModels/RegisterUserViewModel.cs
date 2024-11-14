using System.ComponentModel.DataAnnotations;

namespace HealthConnect.ViewModels
{
    public class RegisterUserViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(10)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
