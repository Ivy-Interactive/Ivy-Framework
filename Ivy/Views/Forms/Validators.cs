using System.ComponentModel.DataAnnotations;

namespace Ivy.Views.Forms;

/// <summary>Utility methods for creating form field validators.</summary>
public static class Validators
{
    public static Func<object?, (bool, string)> CreateEmailValidator(string fieldName)
    {
        var emailValidator = new EmailAddressAttribute();
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
                    : (false, result?.ErrorMessage ?? "Please enter a valid email address");
            }
            catch
            {
                return (false, "Please enter a valid email address");
            }
        };
    }
}

