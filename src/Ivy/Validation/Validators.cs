using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Ivy;
using Ivy.Widgets.Inputs;

namespace Ivy.Validation;

/// <summary>
/// Shared validation for forms. Email, tel, url, and password are validated on blur and submit via FormFieldView.
/// </summary>
public static class Validators
{
    public static Func<object?, (bool, string)> CreateEmailValidator(string fieldName)
    {
        return email =>
        {
            if (email is not string emailStr || string.IsNullOrWhiteSpace(emailStr))
                return (true, "");

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

    public static Func<object?, (bool, string)> CreateTelValidator(string fieldName)
    {
        return value =>
        {
            if (value is not string s || string.IsNullOrWhiteSpace(s))
                return (true, "");

            var digitsOnly = Regex.Replace(s, @"\D", "");
            if (digitsOnly.Length < 7 || digitsOnly.Length > 15)
                return (false, "Please enter a valid phone number");
            if (!Regex.IsMatch(s, @"^[\d\s+\-().]+$"))
                return (false, "Please enter a valid phone number");
            return (true, "");
        };
    }

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

    public static Func<object?, (bool, string)> CreatePasswordValidator(string fieldName, int minLength = 8)
    {
        return value =>
        {
            if (value is not string s || string.IsNullOrWhiteSpace(s))
                return (true, "");

            if (s.Length < minLength)
                return (false, $"Password must be at least {minLength} characters");
            return (true, "");
        };
    }

    public static Func<object?, (bool, string)>? ForVariant(TextInputs variant, string fieldName)
    {
        return variant switch
        {
            TextInputs.Email => CreateEmailValidator(fieldName),
            TextInputs.Tel => CreateTelValidator(fieldName),
            TextInputs.Url => CreateUrlValidator(fieldName),
            TextInputs.Password => CreatePasswordValidator(fieldName),
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

    public static (bool isValid, string? errorMessage) ValidateValue(object? value, TextInputs variant, string? label)
    {
        var validator = ForVariant(variant, label ?? "");
        if (validator == null)
            return (true, null);
        var (valid, message) = validator(value);
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
