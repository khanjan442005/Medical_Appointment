using System.ComponentModel.DataAnnotations;

namespace Doctor_Appointment_System.Models;

public static class PortalValidationRules
{
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 20;
    public const string EmailRequiredMessage = "Email is required.";
    public const string EmailInvalidMessage = "Enter a valid email address.";
    public const string EmailNoSpacesMessage = "Email must not contain spaces.";
    public const string PasswordRequiredMessage = "Password is required.";
    public const string NewPasswordRequiredMessage = "New password is required.";
    public const string PasswordLengthMessage = "Password must be at least 8 characters.";
    public const string PasswordMaxLengthMessage = "Password must not exceed 20 characters.";
    public const string PasswordUppercaseMessage = "Password must include at least one uppercase letter.";
    public const string PasswordLowercaseMessage = "Password must include at least one lowercase letter.";
    public const string PasswordNumberMessage = "Password must include at least one number.";
    public const string PasswordSpecialCharacterMessage = "Password must include at least one special character (@, #, $, etc.).";
    public const string ConfirmPasswordRequiredMessage = "Confirm password is required.";
    public const string PasswordMismatchMessage = "Passwords do not match.";
}

public sealed class LoginInputModel
{
    public static IReadOnlyList<string> SupportedRoles { get; } = ["Patient", "Doctor", "Admin"];

    [Required(ErrorMessage = "Please select a role.")]
    [RegularExpression("Patient|Doctor|Admin", ErrorMessage = "Please select a valid role.")]
    public string Role { get; set; } = "Patient";

    [Required(ErrorMessage = PortalValidationRules.EmailRequiredMessage)]
    [StrictEmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.PasswordRequiredMessage)]
    public string Password { get; set; } = string.Empty;
}

public sealed class ForgotPasswordInputModel
{
    [Required(ErrorMessage = PortalValidationRules.EmailRequiredMessage)]
    [StrictEmailAddress]
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordInputModel
{
    [Required(ErrorMessage = PortalValidationRules.EmailRequiredMessage)]
    [StrictEmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.NewPasswordRequiredMessage)]
    [MinLength(PortalValidationRules.PasswordMinLength, ErrorMessage = PortalValidationRules.PasswordLengthMessage)]
    [MaxLength(PortalValidationRules.PasswordMaxLength, ErrorMessage = PortalValidationRules.PasswordMaxLengthMessage)]
    [StrongPassword]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.ConfirmPasswordRequiredMessage)]
    [Compare(nameof(Password), ErrorMessage = PortalValidationRules.PasswordMismatchMessage)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class PatientRegistrationInputModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 80 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.EmailRequiredMessage)]
    [StrictEmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.PasswordRequiredMessage)]
    [MinLength(PortalValidationRules.PasswordMinLength, ErrorMessage = PortalValidationRules.PasswordLengthMessage)]
    [MaxLength(PortalValidationRules.PasswordMaxLength, ErrorMessage = PortalValidationRules.PasswordMaxLengthMessage)]
    [StrongPassword]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.ConfirmPasswordRequiredMessage)]
    [Compare(nameof(Password), ErrorMessage = PortalValidationRules.PasswordMismatchMessage)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class DoctorRegistrationInputModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 80 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.EmailRequiredMessage)]
    [StrictEmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Specialization is required.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Specialization must be between 2 and 80 characters.")]
    public string Specialization { get; set; } = string.Empty;

    [Required(ErrorMessage = "License number is required.")]
    [StringLength(40, MinimumLength = 5, ErrorMessage = "License number must be between 5 and 40 characters.")]
    public string License { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.PasswordRequiredMessage)]
    [MinLength(PortalValidationRules.PasswordMinLength, ErrorMessage = PortalValidationRules.PasswordLengthMessage)]
    [MaxLength(PortalValidationRules.PasswordMaxLength, ErrorMessage = PortalValidationRules.PasswordMaxLengthMessage)]
    [StrongPassword]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.ConfirmPasswordRequiredMessage)]
    [Compare(nameof(Password), ErrorMessage = PortalValidationRules.PasswordMismatchMessage)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class AdminRegistrationInputModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 80 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.EmailRequiredMessage)]
    [StrictEmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Admin access code is required.")]
    public string AdminCode { get; set; } = "MEDICORE-ADMIN";

    [Required(ErrorMessage = PortalValidationRules.PasswordRequiredMessage)]
    [MinLength(PortalValidationRules.PasswordMinLength, ErrorMessage = PortalValidationRules.PasswordLengthMessage)]
    [MaxLength(PortalValidationRules.PasswordMaxLength, ErrorMessage = PortalValidationRules.PasswordMaxLengthMessage)]
    [StrongPassword]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.ConfirmPasswordRequiredMessage)]
    [Compare(nameof(Password), ErrorMessage = PortalValidationRules.PasswordMismatchMessage)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class AdminCreateDoctorInputModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 80 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.EmailRequiredMessage)]
    [StrictEmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Specialization is required.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Specialization must be between 2 and 80 characters.")]
    public string Specialization { get; set; } = string.Empty;

    [Required(ErrorMessage = "License number is required.")]
    [StringLength(40, MinimumLength = 5, ErrorMessage = "License number must be between 5 and 40 characters.")]
    public string License { get; set; } = string.Empty;

    [Range(1, 60, ErrorMessage = "Experience must be between 1 and 60 years.")]
    public int ExperienceYears { get; set; } = 5;

    [Required(ErrorMessage = "Hospital name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Hospital name must be between 2 and 120 characters.")]
    public string HospitalName { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "City must be between 2 and 80 characters.")]
    public string City { get; set; } = string.Empty;

    [Range(typeof(decimal), "100", "5000", ErrorMessage = "Consultation fee must be between INR 100 and INR 5000.")]
    public decimal ConsultationFee { get; set; } = 700m;

    [Required(ErrorMessage = PortalValidationRules.PasswordRequiredMessage)]
    [MinLength(PortalValidationRules.PasswordMinLength, ErrorMessage = PortalValidationRules.PasswordLengthMessage)]
    [MaxLength(PortalValidationRules.PasswordMaxLength, ErrorMessage = PortalValidationRules.PasswordMaxLengthMessage)]
    [StrongPassword]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = PortalValidationRules.ConfirmPasswordRequiredMessage)]
    [Compare(nameof(Password), ErrorMessage = PortalValidationRules.PasswordMismatchMessage)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
