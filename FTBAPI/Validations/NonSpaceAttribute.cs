using System.ComponentModel.DataAnnotations;

namespace FTBAPI.Validations
{
    public class NonSpaceAttribute:ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value?.ToString()?.Contains(" ") == true)
            {
                return new ValidationResult("該字串不可包含空格!!!");
            }
            return ValidationResult.Success;
        }
    }
}
