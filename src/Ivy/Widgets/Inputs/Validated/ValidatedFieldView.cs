using Ivy;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy.Widgets.Inputs.Validated;

/// <summary>
/// View that wraps a validating TextInput (Email, Password, Tel, Url) in a Field and wires auto-validation on blur.
/// Use via ToEmailInput(), ToPasswordInput(), ToUrlInput(), ToTelInput().
/// </summary>
public sealed class ValidatedFieldView : ViewBase
{
    private readonly IAnyState _state;
    private readonly TextInputVariants _variant;
    private readonly string? _placeholder;
    private readonly bool _disabled;
    private readonly string? _label;
    private readonly string? _description;
    private readonly bool _required;
    private readonly string? _help;
    private readonly Scale _scale;
    private readonly int? _maxLength;
    private readonly bool _nullable;
    private readonly string? _invalid;

    public ValidatedFieldView(IAnyState state, TextInputVariants variant, string? placeholder = null, bool disabled = false, string? label = null, string? description = null, bool required = false, string? help = null, Scale scale = Shared.Scale.Medium, int? maxLength = null, bool nullable = false, string? invalid = null)
    {
        _state = state;
        _variant = variant;
        _placeholder = placeholder;
        _disabled = disabled;
        _label = label;
        _description = description;
        _required = required;
        _help = help;
        _scale = scale;
        _maxLength = maxLength;
        _nullable = nullable;
        _invalid = invalid;
    }

    public ValidatedFieldView Label(string label) => new(_state, _variant, _placeholder, _disabled, label, _description, _required, _help, _scale, _maxLength, _nullable, _invalid);
    public ValidatedFieldView Description(string description) => new(_state, _variant, _placeholder, _disabled, _label, description, _required, _help, _scale, _maxLength, _nullable, _invalid);
    public ValidatedFieldView Required() => new(_state, _variant, _placeholder, _disabled, _label, _description, true, _help, _scale, _maxLength, _nullable, _invalid);
    public ValidatedFieldView Help(string help) => new(_state, _variant, _placeholder, _disabled, _label, _description, _required, help, _scale, _maxLength, _nullable, _invalid);
    public ValidatedFieldView Placeholder(string placeholder) => new(_state, _variant, placeholder, _disabled, _label, _description, _required, _help, _scale, _maxLength, _nullable, _invalid);
    public ValidatedFieldView Disabled(bool disabled = true) => new(_state, _variant, _placeholder, disabled, _label, _description, _required, _help, _scale, _maxLength, _nullable, _invalid);
    public ValidatedFieldView MaxLength(int maxLength) => new(_state, _variant, _placeholder, _disabled, _label, _description, _required, _help, _scale, maxLength, _nullable, _invalid);
    public ValidatedFieldView Nullable(bool nullable = true) => new(_state, _variant, _placeholder, _disabled, _label, _description, _required, _help, _scale, _maxLength, nullable, _invalid);
    public ValidatedFieldView Invalid(string? invalid) => new(_state, _variant, _placeholder, _disabled, _label, _description, _required, _help, _scale, _maxLength, _nullable, invalid);
    public ValidatedFieldView Small() => new(_state, _variant, _placeholder, _disabled, _label, _description, _required, _help, Shared.Scale.Small, _maxLength, _nullable, _invalid);
    public ValidatedFieldView Medium() => new(_state, _variant, _placeholder, _disabled, _label, _description, _required, _help, Shared.Scale.Medium, _maxLength, _nullable, _invalid);
    public ValidatedFieldView Large() => new(_state, _variant, _placeholder, _disabled, _label, _description, _required, _help, Shared.Scale.Large, _maxLength, _nullable, _invalid);

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
        if (_maxLength is { } maxLen)
            input = ((TextInputBase)input).MaxLength(maxLen);
        if (_nullable)
            input = ((TextInputBase)input).Nullable(true);

        return new Field(input, _label, _description, _required, _help, _scale);
    }
}
