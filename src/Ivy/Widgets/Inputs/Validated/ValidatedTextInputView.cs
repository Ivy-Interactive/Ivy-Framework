using Ivy;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy.Widgets.Inputs.Validated;

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
                var (isValid, errorMessage) = TextInputExtensions.ValidateForVariant(value, Variant);
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
