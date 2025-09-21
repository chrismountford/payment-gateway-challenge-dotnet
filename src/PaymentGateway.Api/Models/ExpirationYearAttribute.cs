using System.ComponentModel.DataAnnotations;

public class ExpirationYearAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var currentYear = DateTime.Now.Year;

        if ((int)value < currentYear || value == null)
        {
            return new ValidationResult("Expiration year must not be in the past");
        }

        return ValidationResult.Success;
    }
}