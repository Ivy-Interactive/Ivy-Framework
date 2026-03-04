using System.Net.Mail;
using System.Text.RegularExpressions;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy.Widgets.Inputs.Validated;

/// <summary>
/// Validation logic for Email, Password, Tel, Url text input variants.
/// </summary>
public static class TextInputValidation
{
    /// <summary>
    /// Validates value for the given text input variant. Returns (true, null) if valid, (false, errorMessage) if invalid.
    /// Used by ValidatedFieldView and FormFieldView via Validators.
    /// </summary>
    public static (bool isValid, string? errorMessage) ValidateForVariant(object? value, TextInputVariants variant)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
            return (true, null);

        return variant switch
        {
            TextInputVariants.Email => ValidateEmail(s),
            TextInputVariants.Password => ValidatePassword(s),
            TextInputVariants.Tel => ValidateTel(s),
            TextInputVariants.Url => ValidateUrl(s),
            _ => (true, null)
        };
    }

    private static (bool valid, string? error) ValidateEmail(string s)
    {
        try
        {
            var addr = new MailAddress(s);
            if (!addr.Host.Contains('.'))
                return (false, "Please enter a valid email address");
            return (true, null);
        }
        catch (FormatException)
        {
            return (false, "Please enter a valid email address");
        }
    }

    private static (bool valid, string? error) ValidatePassword(string s, int minLength = 8)
    {
        if (s.Length < minLength)
            return (false, $"Password must be at least {minLength} characters");
        return (true, null);
    }

    private static (bool valid, string? error) ValidateTel(string s)
    {
        var digitsOnly = Regex.Replace(s, @"\D", "");
        if (digitsOnly.Length < 7 || digitsOnly.Length > 15)
            return (false, "Please enter a valid phone number");
        if (!Regex.IsMatch(s, @"^[\d\s+\-().]+$"))
            return (false, "Please enter a valid phone number");
        return (true, null);
    }

    private static (bool valid, string? error) ValidateUrl(string s)
    {
        if (!Uri.TryCreate(s, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return (false, "Please enter a valid URL");
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return (false, "Please enter a valid URL (http or https)");
        return (true, null);
    }
}
