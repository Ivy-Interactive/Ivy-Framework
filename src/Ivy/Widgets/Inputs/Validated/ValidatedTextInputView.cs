using System.Net.Mail;
using System.Text.RegularExpressions;
using Ivy;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
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
    /// Used by ValidatedTextInputView, ValidatedFieldView, and FormFieldView via Validators.
    /// </summary>
    public static (bool isValid, string? errorMessage) ValidateForVariant(object? value, TextInputs variant)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
            return (true, null);

        return variant switch
        {
            TextInputs.Email => ValidateEmail(s),
            TextInputs.Password => ValidatePassword(s),
            TextInputs.Tel => ValidateTel(s),
            TextInputs.Url => ValidateUrl(s),
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

/// <summary>
/// View that builds a validating TextInput (Email, Password, Tel, Url) with validation on blur.
/// </summary>
public sealed class ValidatedTextInputView : ViewBase
{
    public IAnyState State { get; }
    public TextInputs Variant { get; }
    public string? Placeholder { get; }
    public bool Disabled { get; }

    public ValidatedTextInputView(IAnyState state, TextInputs variant, string? placeholder = null, bool disabled = false)
    {
        State = state;
        Variant = variant;
        Placeholder = placeholder;
        Disabled = disabled;
    }

    public ValidatedTextInputView WithPlaceholder(string placeholder) => new(State, Variant, placeholder, Disabled);
    public ValidatedTextInputView WithDisabled(bool disabled = true) => new(State, Variant, Placeholder, disabled);

    public override object? Build()
    {
        var invalidState = UseState(default(string?));
        var blurOnceState = UseState(false);

        UseEffect(() =>
        {
            if (blurOnceState.Value)
            {
                var value = State.As<object>().Value;
                var (isValid, errorMessage) = TextInputValidation.ValidateForVariant(value, Variant);
                invalidState.Set(isValid ? null! : errorMessage ?? "");
            }
        }, State, blurOnceState);

        void OnBlur(Event<IAnyInput> _) => blurOnceState.Set(true);

        var input = State.ToTextInput(Placeholder, Disabled, Variant);
        var validated = (TextInputBase)input.Invalid(invalidState.Value ?? "").HandleBlur(OnBlur);
        validated.FromValidatedView = true;
        return new Fragment(validated);
    }
}
