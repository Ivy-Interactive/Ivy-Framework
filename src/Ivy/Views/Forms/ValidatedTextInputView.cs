using System;
using Ivy;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Validation;
using Ivy.Widgets.Inputs;

namespace Ivy.Views.Forms;

/// <summary>
/// A view that renders a TextInput (any variant: Email, Tel, Url, Password, etc.) with validation on blur,
/// using the same <see cref="Validators"/> as forms. Use when the input is outside a form so it still validates.
/// </summary>
public class ValidatedTextInputView(
    IAnyState state,
    TextInputs variant,
    string label,
    string? placeholder = null,
    bool required = false,
    string? description = null,
    Func<object?, (bool, string)>[]? validators = null,
    Scale scale = Scale.Medium)
    : ViewBase
{
    public override object? Build()
    {
        IAnyState inputState = Context.UseClonedAnyState(state);
        var invalidState = UseState((string?)null!);
        var blurOnceState = UseState(false);

        var input = inputState.ToTextInput(placeholder, false, variant);

        UseEffect(() =>
        {
            var value = inputState.As<object>().Value;
            if (blurOnceState.Value)
            {
                var (isValid, errorMessage) = Validators.RunValidation(value, input, label, validators);
                invalidState.Set(isValid ? null! : errorMessage ?? "");
            }
            state.As<object>().Set(value);
        }, inputState, blurOnceState);

        void OnBlur(Event<IAnyInput> _)
        {
            blurOnceState.Set(true);
        }

        input = input.Invalid(invalidState.Value).HandleBlur(OnBlur);

        if (!string.IsNullOrEmpty(placeholder))
        {
            input.Placeholder = placeholder;
        }

        if (scale != Scale.Medium)
        {
            WidgetBaseExtensions.SetScaleViaReflection(input, scale);
        }

        return new Field(input, label, description, required, null, scale);
    }
}

/// <summary>
/// Extension methods to create a validated TextInput view (validates on blur – same logic as forms).
/// Use state.ToEmailInput(this, "Email") etc. from your view's Build() so the text input validates (e.g. email must contain @ and .).
/// </summary>
public static class ValidatedTextInputExtensions
{
    /// <summary>Validated email TextInput (validates on blur; use from a view: state.ToEmailInput(this, "Email").</summary>
    public static ValidatedTextInputView ToEmailInput(this IAnyState state, ViewBase view, string label, string? placeholder = null, bool required = false, string? description = null, Scale scale = Scale.Medium)
        => new(state, TextInputs.Email, label, placeholder, required, description, null, scale);

    /// <summary>Validated password TextInput (validates on blur; use from a view: state.ToPasswordInput(this, "Password").</summary>
    public static ValidatedTextInputView ToPasswordInput(this IAnyState state, ViewBase view, string label, string? placeholder = null, bool required = false, string? description = null, Scale scale = Scale.Medium)
        => new(state, TextInputs.Password, label, placeholder, required, description, null, scale);

    /// <summary>Validated URL TextInput (validates on blur; use from a view: state.ToUrlInput(this, "Url").</summary>
    public static ValidatedTextInputView ToUrlInput(this IAnyState state, ViewBase view, string label, string? placeholder = null, bool required = false, string? description = null, Scale scale = Scale.Medium)
        => new(state, TextInputs.Url, label, placeholder, required, description, null, scale);

    /// <summary>Validated telephone TextInput (validates on blur; use from a view: state.ToTelInput(this, "Phone").</summary>
    public static ValidatedTextInputView ToTelInput(this IAnyState state, ViewBase view, string label, string? placeholder = null, bool required = false, string? description = null, Scale scale = Scale.Medium)
        => new(state, TextInputs.Tel, label, placeholder, required, description, null, scale);

    /// <summary>Validated TextInput for the given variant (use from a view: state.ToTextInput(this, TextInputs.Email, "Email").</summary>
    public static ValidatedTextInputView ToTextInput(this IAnyState state, ViewBase view, TextInputs variant, string label, string? placeholder = null, bool required = false, string? description = null, Func<object?, (bool, string)>[]? validators = null, Scale scale = Scale.Medium)
        => new(state, variant, label, placeholder, required, description, validators, scale);

    /// <summary>Returns a view that renders an email TextInput with validation on blur.</summary>
    public static ValidatedTextInputView ToValidatedEmailInput(this IAnyState state, string label, string? placeholder = null, bool required = false, string? description = null, Scale scale = Scale.Medium)
        => new(state, TextInputs.Email, label, placeholder, required, description, null, scale);

    /// <summary>Returns a view that renders a password TextInput with validation on blur.</summary>
    public static ValidatedTextInputView ToValidatedPasswordInput(this IAnyState state, string label, string? placeholder = null, bool required = false, string? description = null, Scale scale = Scale.Medium)
        => new(state, TextInputs.Password, label, placeholder, required, description, null, scale);

    /// <summary>Returns a view that renders a URL TextInput with validation on blur.</summary>
    public static ValidatedTextInputView ToValidatedUrlInput(this IAnyState state, string label, string? placeholder = null, bool required = false, string? description = null, Scale scale = Scale.Medium)
        => new(state, TextInputs.Url, label, placeholder, required, description, null, scale);

    /// <summary>Returns a view that renders a telephone TextInput with validation on blur.</summary>
    public static ValidatedTextInputView ToValidatedTelInput(this IAnyState state, string label, string? placeholder = null, bool required = false, string? description = null, Scale scale = Scale.Medium)
        => new(state, TextInputs.Tel, label, placeholder, required, description, null, scale);

    /// <summary>Returns a view that renders a TextInput with the given variant and validation on blur (Email, Tel, Url, Password).</summary>
    public static ValidatedTextInputView ToValidatedTextInput(this IAnyState state, TextInputs variant, string label, string? placeholder = null, bool required = false, string? description = null, Func<object?, (bool, string)>[]? validators = null, Scale scale = Scale.Medium)
        => new(state, variant, label, placeholder, required, description, validators, scale);
}
