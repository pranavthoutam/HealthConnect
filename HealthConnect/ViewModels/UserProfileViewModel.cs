namespace HealthConnect.ViewModels
{
    public class UserProfileViewModel
    {
        [Required(ErrorMessage = "User name is required.")]
        public string UserName { get; set; }

        public Gender? Gender { get; set; }
        public BloodGroup? BloodGroup { get; set; }

        [DataType(DataType.Date)]
        [CustomValidation(typeof(UserProfileViewModel), nameof(ValidateDateOfBirth))]
        public string? DateofBirth { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(\+91|91)?[6-9]\d{9}$", ErrorMessage = "Enter a valid Indian phone number.")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [RegularExpression(@"^.*\.(jpg|jpeg|png)$", ErrorMessage = "Only image files (jpg, jpeg, png) are allowed.")]
        public IFormFile? ProfilePhoto { get; set; }

        [StringLength(100, ErrorMessage = "House number cannot exceed 100 characters.")]
        public string? HouseNumber { get; set; }

        [StringLength(100, ErrorMessage = "Street name cannot exceed 100 characters.")]
        public string? Street { get; set; }

        [StringLength(100, ErrorMessage = "City name cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "City name must contain only letters.")]
        public string? City { get; set; }

        [StringLength(6, MinimumLength = 6, ErrorMessage = "Postal code must be exactly 6 digits.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Postal code must contain only numbers.")]
        public string? PostalCode { get; set; }

        [StringLength(50, ErrorMessage = "State name cannot exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "State name must contain only letters.")]
        public string? State { get; set; }

        [StringLength(50, ErrorMessage = "Country name cannot exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Country name must contain only letters.")]
        public string? Country { get; set; }

        // New property for Email that is read-only in the view
        public string Email { get; set; }

        // Custom validation for DateofBirth
        public static ValidationResult? ValidateDateOfBirth(string date, ValidationContext context)
        {
            if (string.IsNullOrEmpty(date))
                return ValidationResult.Success;

            if (DateTime.TryParse(date, out var parsedDate))
            {
                if (parsedDate > DateTime.Now)
                {
                    return new ValidationResult("Date of birth cannot be a future date.");
                }
                return ValidationResult.Success;
            }
            return new ValidationResult("Invalid date format.");
        }
    }
}
