using System.Net.Mail;
using System.Text.RegularExpressions;
using Ivy;

namespace Ivy.Views.Forms;

/// <summary>
/// Common validators for form fields. Use with FormBuilder or manual validation.
/// </summary>
public static class Validators
{
    /// <summary>
    /// Validates email format. Empty values are considered valid (use Required validator for that).
    /// </summary>
    public static Func<object?, (bool, string)> CreateEmailValidator(string fieldName)
    {
        return email =>
        {
            if (email is not string emailStr || string.IsNullOrWhiteSpace(emailStr))
                return (true, ""); // Empty is handled by Required validator

            try
            {
                var addr = new MailAddress(emailStr);

                if (!addr.Host.Contains('.'))
                    return (false, "Please enter a valid email address");

                return (true, "");
            }
            catch (FormatException)
            {
                return (false, "Please enter a valid email address");
            }
        };
    }

    /// <summary>
    /// Validates telephone format. Allows digits, spaces, +, -, (), . and requires at least 7 digits.
    /// Empty values are considered valid (use Required validator for that).
    /// </summary>
    public static Func<object?, (bool, string)> CreateTelValidator(string fieldName)
    {
        return value =>
        {
            if (value is not string s || string.IsNullOrWhiteSpace(s))
                return (true, "");

            var digitsOnly = Regex.Replace(s, @"\D", "");
            if (digitsOnly.Length < 7 || digitsOnly.Length > 15)
                return (false, "Please enter a valid phone number");

            // Allow only common phone characters
            if (!Regex.IsMatch(s, @"^[\d\s+\-().]+$"))
                return (false, "Please enter a valid phone number");

            return (true, "");
        };
    }

    /// <summary>
    /// Validates URL format (http/https). Empty values are considered valid (use Required validator for that).
    /// </summary>
    public static Func<object?, (bool, string)> CreateUrlValidator(string fieldName)
    {
        return value =>
        {
            if (value is not string s || string.IsNullOrWhiteSpace(s))
                return (true, "");

            if (!Uri.TryCreate(s, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
                return (false, "Please enter a valid URL");

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return (false, "Please enter a valid URL (http or https)");

            return (true, "");
        };
    }

    /// <summary>
    /// Returns a format validator for the given TextInput variant, or null if the variant has no format validation (e.g. Text, Password, Search).
    /// Use when building forms manually with ToEmailInput(), ToTelInput(), ToUrlInput() or .Variant() to ensure format is validated.
    /// </summary>
    public static Func<object?, (bool, string)>? ForVariant(TextInputs variant, string fieldName)
    {
        return variant switch
        {
            TextInputs.Email => CreateEmailValidator(fieldName),
            TextInputs.Tel => CreateTelValidator(fieldName),
            TextInputs.Url => CreateUrlValidator(fieldName),
            _ => null
        };
    }
}

