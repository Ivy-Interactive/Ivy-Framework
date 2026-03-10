using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
namespace Ivy.Validation;

/// <summary>
/// Shared validation for forms. Email, tel, url, and password are validated on blur and submit via FormFieldView.
/// </summary>
public static class Validators
{
    /// <summary>Returns (true, null) if valid, (false, errorMessage) if invalid. Used by form validation and TextInput on-blur.</summary>
    public static (bool isValid, string? errorMessage) ValidateForVariant(object? value, TextInputVariants variant, int passwordMinLength = 8)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
            return (true, null);
        var err = variant switch
        {
            TextInputVariants.Email => ValidateEmail(value),
            TextInputVariants.Password => ValidatePassword(value, passwordMinLength),
            TextInputVariants.Tel => ValidateTel(value),
            TextInputVariants.Url => ValidateUrl(value),
            _ => null
        };
        return (err == null, err);
    }

    private static string? ValidateEmail(object? value)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return null;
        try
        {
            var addr = new MailAddress(s);
            return addr.Host.Contains('.') ? null : "Please enter a valid email address";
        }
        catch (FormatException) { return "Please enter a valid email address"; }
    }

    private static string? ValidatePassword(object? value, int minLength = 8)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return null;
        return s.Length >= minLength ? null : $"Password must be at least {minLength} characters";
    }

    private static string? ValidateTel(object? value)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return null;
        var digitsOnly = Regex.Replace(s, @"\D", "");
        if (digitsOnly.Length < 7 || digitsOnly.Length > 15) return "Please enter a valid phone number";
        return Regex.IsMatch(s, @"^[\d\s+\-().]+$") ? null : "Please enter a valid phone number";
    }

    private static string? ValidateUrl(object? value)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return null;
        if (!Uri.TryCreate(s, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri) return "Please enter a valid URL";
        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps ? null : "Please enter a valid URL (http or https)";
    }

    public static Func<object?, (bool, string)> CreateEmailValidator(string fieldName) =>
        v => { var (ok, err) = ValidateForVariant(v, TextInputVariants.Email); return (ok, err ?? ""); };

    public static Func<object?, (bool, string)> CreateTelValidator(string fieldName) =>
        v => { var (ok, err) = ValidateForVariant(v, TextInputVariants.Tel); return (ok, err ?? ""); };

    public static Func<object?, (bool, string)> CreateUrlValidator(string fieldName) =>
        v => { var (ok, err) = ValidateForVariant(v, TextInputVariants.Url); return (ok, err ?? ""); };

    public static Func<object?, (bool, string)> CreatePasswordValidator(string fieldName, int minLength = 8) =>
        v => { var (ok, err) = ValidateForVariant(v, TextInputVariants.Password, minLength); return (ok, err ?? ""); };

    public static Func<object?, (bool, string)>? ForVariant(TextInputVariants variant, string fieldName)
    {
        return variant switch
        {
            TextInputVariants.Email => CreateEmailValidator(fieldName),
            TextInputVariants.Tel => CreateTelValidator(fieldName),
            TextInputVariants.Url => CreateUrlValidator(fieldName),
            TextInputVariants.Password => CreatePasswordValidator(fieldName),
            _ => null
        };
    }

    public static Func<object?, (bool, string)>[] GetEffectiveValidators(IAnyInput input, string? label, IEnumerable<Func<object?, (bool, string)>?>? existingValidators)
    {
        var list = (existingValidators ?? []).Where(v => v != null).Cast<Func<object?, (bool, string)>>().ToList();
        if (input is IAnyTextInput textInput)
        {
            var v = ForVariant(textInput.Variant, label ?? "");
            if (v != null)
                list.Add(v);
        }
        return list.ToArray();
    }

    public static (bool isValid, string? errorMessage) ValidateValue(object? value, TextInputVariants variant, string? label, int passwordMinLength = 8)
    {
        var (valid, message) = ValidateForVariant(value, variant, passwordMinLength);
        return (valid, valid ? null : message);
    }

    public static (bool isValid, string? errorMessage) RunValidation(object? value, IAnyInput input, string? label, IEnumerable<Func<object?, (bool, string)>?>? existingValidators)
    {
        var validators = GetEffectiveValidators(input, label, existingValidators);
        foreach (var validator in validators)
        {
            var (valid, message) = validator(value);
            if (!valid)
                return (false, message);
        }
        return (true, null);
    }
}
