using System.ComponentModel.DataAnnotations;

namespace Doctor_Appointment_System.Models;

public sealed class StrictEmailAddressAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var email = value?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email))
        {
            return ValidationResult.Success;
        }

        if (email.Any(char.IsWhiteSpace))
        {
            return new ValidationResult(PortalValidationRules.EmailNoSpacesMessage);
        }

        var validator = new EmailAddressAttribute();
        return validator.IsValid(email)
            ? ValidationResult.Success
            : new ValidationResult(PortalValidationRules.EmailInvalidMessage);
    }
}

public sealed class StrongPasswordAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var password = value?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(password))
        {
            return ValidationResult.Success;
        }

        if (!password.Any(char.IsUpper))
        {
            return new ValidationResult(PortalValidationRules.PasswordUppercaseMessage);
        }

        if (!password.Any(char.IsLower))
        {
            return new ValidationResult(PortalValidationRules.PasswordLowercaseMessage);
        }

        if (!password.Any(char.IsDigit))
        {
            return new ValidationResult(PortalValidationRules.PasswordNumberMessage);
        }

        var hasSpecialCharacter = password.Any(character => !char.IsLetterOrDigit(character) && !char.IsWhiteSpace(character));
        return hasSpecialCharacter
            ? ValidationResult.Success
            : new ValidationResult(PortalValidationRules.PasswordSpecialCharacterMessage);
    }
}
