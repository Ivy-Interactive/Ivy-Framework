using System.ComponentModel.DataAnnotations;

namespace Ivy.Views.Forms;

/// <summary>Utility methods for creating form field validators.</summary>
public static class Validators
{
    /// <summary>Creates an email validator using EmailAddressAttribute for proper email validation.</summary>
    public static Func<object?, (bool, string)> CreateEmailValidator(string fieldName)
    {
        const string invalidEmailMessage = "Please enter a valid email address";
        var emailValidator = new EmailAddressAttribute
        {
            ErrorMessage = invalidEmailMessage
        };

        return email =>
        {
            if (email is not string emailStr || string.IsNullOrWhiteSpace(emailStr))
                return (true, ""); // Empty is handled by Required validator

            try
            {
                var validationContext = new ValidationContext(new { })
                {
                    MemberName = fieldName,
                    DisplayName = fieldName
                };
                var result = emailValidator.GetValidationResult(emailStr, validationContext);
                return result == ValidationResult.Success
                    ? (true, "")
                    : (false, invalidEmailMessage);
            }
            catch
            {
                return (false, invalidEmailMessage);
            }
        };
    }

    /// <summary>Creates a password validator enforcing minimum complexity rules.</summary>
    /// <param name="minLength">Minimum length required for a valid password.</param>
    public static Func<object?, (bool, string)> CreatePasswordValidator(int minLength = 8)
    {
        string invalidPasswordMessage = $"Password must be at least {minLength} characters long";

        return password =>
        {
            if (password is not string passwordStr || string.IsNullOrWhiteSpace(passwordStr))
            {
                return (true, ""); // Empty is handled by Required validator
            }

            return passwordStr.Length >= minLength
                ? (true, "")
                : (false, invalidPasswordMessage);
        };
    }
}

