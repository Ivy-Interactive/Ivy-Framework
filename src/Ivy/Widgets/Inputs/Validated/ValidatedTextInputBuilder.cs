using Ivy;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy.Widgets.Inputs.Validated;

/// <summary>
/// Builder/view for validated text inputs (Email, Tel, Url, Password). When added to a layout, builds a single
/// validated TextInput (validation on blur; no Field). Use .WithField() to get a validated Field instead.
/// Returned by state.ToEmailInput(), .ToTelInput(), .ToUrlInput(), .ToPasswordInput() when called with no placeholder.
/// </summary>
public sealed class ValidatedTextInputBuilder : ViewBase
{
    private readonly IAnyState _state;
    private readonly TextInputVariants _variant;
    private readonly string? _placeholder;
    private readonly bool _disabled;
    private readonly string? _invalid;
    private readonly bool _nullable;
    private readonly Scale _scale;

    public ValidatedTextInputBuilder(IAnyState state, TextInputVariants variant, string? placeholder = null, bool disabled = false, string? invalid = null, bool nullable = false, Scale scale = Shared.Scale.Medium)
    {
        _state = state;
        _variant = variant;
        _placeholder = placeholder;
        _disabled = disabled;
        _invalid = invalid;
        _nullable = nullable;
        _scale = scale;
    }

    public ValidatedTextInputBuilder Placeholder(string placeholder) =>
        new(_state, _variant, placeholder, _disabled, _invalid, _nullable, _scale);

    public ValidatedTextInputBuilder Disabled(bool disabled = true) =>
        new(_state, _variant, _placeholder, disabled, _invalid, _nullable, _scale);

    public ValidatedTextInputBuilder Invalid(string? invalid) =>
        new(_state, _variant, _placeholder, _disabled, invalid, _nullable, _scale);

    public ValidatedTextInputBuilder Nullable(bool? nullable = true) =>
        new(_state, _variant, _placeholder, _disabled, _invalid, nullable ?? true, _scale);

    public ValidatedTextInputBuilder Small() => new(_state, _variant, _placeholder, _disabled, _invalid, _nullable, Shared.Scale.Small);
    public ValidatedTextInputBuilder Medium() => new(_state, _variant, _placeholder, _disabled, _invalid, _nullable, Shared.Scale.Medium);
    public ValidatedTextInputBuilder Large() => new(_state, _variant, _placeholder, _disabled, _invalid, _nullable, Shared.Scale.Large);

    /// <summary>Returns a validated field (validation on blur; when invalid, the text input and field show invalid).</summary>
    public ValidatedFieldView WithField() =>
        new(_state, _variant, _placeholder, _disabled, invalid: _invalid, nullable: _nullable, scale: _scale);

    public override object? Build()
    {
        var invalidState = UseState(default(string?));
        var blurOnceState = UseState(false);

        UseEffect(() =>
        {
            if (blurOnceState.Value)
            {
                var value = _state.As<object>().Value;
                var (isValid, errorMessage) = TextInputValidation.ValidateForVariant(value, _variant);
                invalidState.Set(isValid ? null! : errorMessage ?? "");
            }
        }, _state, blurOnceState);

        void OnBlur(Event<IAnyInput> _) => blurOnceState.Set(true);

        var invalidMessage = invalidState.Value ?? _invalid ?? "";
        var input = _state.ToTextInput(_placeholder, _disabled, _variant)
            .Invalid(invalidMessage)
            .HandleBlur(OnBlur);
        if (_nullable)
            input = input.Nullable(true);
        if (_scale != Shared.Scale.Medium)
            WidgetBaseExtensions.SetScaleViaReflection(input, _scale);
        return input;
    }
}
