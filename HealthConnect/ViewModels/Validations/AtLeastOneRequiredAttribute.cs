using System.ComponentModel.DataAnnotations;

namespace HealthConnect.ViewModels.Validations
{
    public class AtLeastOneRequiredAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var properties = validationContext.ObjectType.GetProperties();
            bool onlineConsultation = (bool)properties.FirstOrDefault(p => p.Name == "OnlineConsultation")?.GetValue(validationContext.ObjectInstance);
            bool clinicAppoinment = (bool)properties.FirstOrDefault(p => p.Name == "ClinicAppoinment")?.GetValue(validationContext.ObjectInstance);

            if (onlineConsultation || clinicAppoinment)
            {
                return ValidationResult.Success; // Valid if at least one is true
            }

            return new ValidationResult(ErrorMessage ?? "At least one option must be selected.");
        }
    }
}
