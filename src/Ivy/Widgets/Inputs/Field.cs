using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Validation;
using Ivy.Widgets;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Fluent API for Field and validated Field (Label, Description, Required, Help).</summary>
public interface IFieldOptions
{
    IFieldOptions Label(string label);
    IFieldOptions Description(string description);
    IFieldOptions Required(bool required = true);
    IFieldOptions Help(string help);
}

public record Field : WidgetBase<Field>, IFieldOptions
{
    public Field(IAnyInput input, string? label = null, string? description = null, bool required = false, string? help = null, Scale scale = Shared.Scale.Medium) : base([input])
    {
        var labelProp = input.GetType().GetProperty("Label");
        if (labelProp != null && labelProp.PropertyType == typeof(string))
        {
            var inputLabel = (string?)labelProp.GetValue(input);
            labelProp.SetValue(input, inputLabel ?? label);
            label = null;
        }

        var descriptionProp = input.GetType().GetProperty("Description");
        if (descriptionProp != null && descriptionProp.PropertyType == typeof(string))
        {
            var inputDescription = (string?)descriptionProp.GetValue(input);
            descriptionProp.SetValue(input, inputDescription ?? description);
            description = null;
        }

        Label = label;
        Description = description;
        Required = required;
        Help = help;
        Scale = scale;
    }

    internal Field() { }

    [Prop] public string? Label { get; set; }

    [Prop] public string? Description { get; set; }

    [Prop] public bool Required { get; set; }

    [Prop] public string? Help { get; set; }

    public static Field operator |(Field widget, object child)
    {
        throw new NotSupportedException("Field does not support children.");
    }

    IFieldOptions IFieldOptions.Label(string label) => this with { Label = label };
    IFieldOptions IFieldOptions.Description(string description) => this with { Description = description };
    IFieldOptions IFieldOptions.Required(bool required) => this with { Required = required };
    IFieldOptions IFieldOptions.Help(string help) => this with { Help = help };
}

/// <summary>View that wraps an input in a Field and runs BE validation on blur when variant is Email/Tel/Url/Password. Used by WithField(state).</summary>
internal sealed class FieldView : ViewBase, IFieldOptions
{
    private readonly IAnyState _state;
    private readonly IAnyTextInput _input;
    private string? _label;
    private string? _description;
    private bool _required;
    private string? _help;
    private Shared.Scale _scale = Shared.Scale.Medium;

    public FieldView(IAnyState state, IAnyTextInput input)
    {
        _state = state;
        _input = input;
    }

    public IFieldOptions Label(string label) { _label = label; return this; }
    public IFieldOptions Description(string description) { _description = description; return this; }
    public IFieldOptions Required(bool required = true) { _required = required; return this; }
    public IFieldOptions Help(string help) { _help = help; return this; }

    public override object? Build()
    {
        var invalidState = UseState("");
        var blurOnceState = UseState(false);
        var variant = _input.Variant;
        var placeholder = (_input as TextInputBase)?.Placeholder;

        TextInputBase validatedInput = variant switch
        {
            TextInputs.Email => _state.ToEmailInput(placeholder),
            TextInputs.Tel => _state.ToTelInput(placeholder),
            TextInputs.Url => _state.ToUrlInput(placeholder),
            TextInputs.Password => _state.ToPasswordInput(placeholder),
            _ => _state.ToTextInput(placeholder, false, variant)
        };

        UseEffect(() =>
        {
            if (blurOnceState.Value)
            {
                var (isValid, errorMessage) = Validators.RunValidation(_state.As<object>().Value, validatedInput, _label, null);
                invalidState.Set(isValid ? "" : errorMessage ?? "");
            }
        }, _state, blurOnceState);

        validatedInput = validatedInput
            .Invalid(invalidState.Value)
            .HandleBlur(_ => blurOnceState.Set(true));

        if (_scale != Shared.Scale.Medium)
            WidgetBaseExtensions.SetScaleViaReflection(validatedInput, _scale);

        return new Field(validatedInput, _label, _description, _required, _help, _scale);
    }
}

public static class FieldExtensions
{
    public static Field Label(this Field field, string label) => field with { Label = label };

    public static Field Description(this Field field, string description) => field with { Description = description };

    public static Field Help(this Field field, string help) => field with { Help = help };

    public static Field Required(this Field field) => field with { Required = true };

    public static Field WithField(this IAnyInput input) => new Field(input);

    /// <summary>
    /// Wraps the input in a Field. When the input is a variant that supports validation (Email, Tel, Url, Password), BE validation runs on blur automatically — no HandleBlur needed.
    /// </summary>
    public static IFieldOptions WithField(this IAnyInput input, IAnyState state)
    {
        if (input is IAnyTextInput textInput && Validators.ForVariant(textInput.Variant, "") != null)
            return new FieldView(state, textInput);
        return new Field(input);
    }

    /// <summary>
    /// Returns the variant validator for the wrapped input when it is a validatable TextInput (Email, Tel, Url, Password).
    /// Same pattern as <see cref="TextInputExtensions.GetVariantValidator"/> on TextInput; use with Validators.ValidateValue or run the returned validator yourself.
    /// </summary>
    public static Func<object?, (bool, string)>? GetVariantValidator(this Field field)
    {
        if (field.Children is not { Length: > 0 } || field.Children[0] is not IAnyTextInput textInput)
            return null;
        return Validators.ForVariant(textInput.Variant, field.Label ?? "");
    }
}

