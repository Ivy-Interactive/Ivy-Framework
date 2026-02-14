using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy.Widgets.Inputs.Validated;

/// <summary>
/// View that wraps a validating TextInput (Email, Password, Tel, Url) in a Field and wires auto-validation on blur.
/// </summary>
internal sealed class ValidatedFieldView : ViewBase, IFieldWithLabel
{
    private readonly TextInputBase _input;
    private readonly IAnyState _state;
    private readonly TextInputs _variant;
    private readonly string? _label;
    private readonly string? _description;
    private readonly bool _required;
    private readonly string? _help;
    private readonly Scale _scale;

    public ValidatedFieldView(TextInputBase input, IAnyState state, TextInputs variant, string? label = null, string? description = null, bool required = false, string? help = null, Scale scale = Shared.Scale.Medium)
    {
        _input = input;
        _state = state;
        _variant = variant;
        _label = label;
        _description = description;
        _required = required;
        _help = help;
        _scale = scale;
    }

    public ValidatedFieldView Label(string label) => new(_input, _state, _variant, label, _description, _required, _help, _scale);
    public ValidatedFieldView Description(string description) => new(_input, _state, _variant, _label, description, _required, _help, _scale);
    public ValidatedFieldView Required() => new(_input, _state, _variant, _label, _description, true, _help, _scale);
    public ValidatedFieldView Help(string help) => new(_input, _state, _variant, _label, _description, _required, help, _scale);

    public override object? Build()
    {
        var invalidState = UseState(default(string?));
        var blurOnceState = UseState(false);

        UseEffect(() =>
        {
            if (blurOnceState.Value)
            {
                var value = _state.As<object>().Value;
                var (isValid, errorMessage) = TextInputExtensions.ValidateForVariant(value, _variant);
                invalidState.Set(isValid ? null! : errorMessage ?? "");
            }
        }, _state, blurOnceState);

        void OnBlur(Event<IAnyInput> _) => blurOnceState.Set(true);

        var validatedInput = _input
            .Invalid(invalidState.Value ?? "")
            .HandleBlur(OnBlur);

        return new Field(validatedInput, _label, _description, _required, _help, _scale);
    }
}
