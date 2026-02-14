using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Return type of WithField(). Implemented by Field and ValidatedFieldView so Label/Description/Required/Help (and Width when Field) apply without conflicting with Card or other types.
/// </summary>
public interface IFieldWithLabel { }

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

public record Field : WidgetBase<Field>, IFieldWithLabel
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
}

public static class FieldExtensions
{
    public static Field Label(this Field field, string label) => field with { Label = label };

    public static Field Description(this Field field, string description) => field with { Description = description };

    public static Field Help(this Field field, string help) => field with { Help = help };

    public static Field Required(this Field field) => field with { Required = true };

    public static IFieldWithLabel Label(this IFieldWithLabel fieldOrView, string label) =>
        fieldOrView is Field f ? f.Label(label) : fieldOrView is ValidatedFieldView v ? v.Label(label) : fieldOrView;

    public static IFieldWithLabel Description(this IFieldWithLabel fieldOrView, string description) =>
        fieldOrView is Field f ? f.Description(description) : fieldOrView is ValidatedFieldView v ? v.Description(description) : fieldOrView;

    public static IFieldWithLabel Required(this IFieldWithLabel fieldOrView) =>
        fieldOrView is Field f ? f.Required() : fieldOrView is ValidatedFieldView v ? v.Required() : fieldOrView;

    public static IFieldWithLabel Help(this IFieldWithLabel fieldOrView, string help) =>
        fieldOrView is Field f ? f.Help(help) : fieldOrView is ValidatedFieldView v ? v.Help(help) : fieldOrView;

    public static IFieldWithLabel Width(this IFieldWithLabel fieldOrView, Size? width) =>
        fieldOrView is Field f ? f.Width(width) : fieldOrView;

    public static IFieldWithLabel Width(this IFieldWithLabel fieldOrView, int units) =>
        fieldOrView is Field f ? f.Width(units) : fieldOrView;

    public static IFieldWithLabel Width(this IFieldWithLabel fieldOrView, double units) =>
        fieldOrView is Field f ? f.Width(units) : fieldOrView;

    public static IFieldWithLabel Height(this IFieldWithLabel fieldOrView, Size? height) =>
        fieldOrView is Field f ? f.Height(height) : fieldOrView;

    public static IFieldWithLabel Height(this IFieldWithLabel fieldOrView, int units) =>
        fieldOrView is Field f ? f.Height(units) : fieldOrView;

    public static IFieldWithLabel Height(this IFieldWithLabel fieldOrView, double units) =>
        fieldOrView is Field f ? f.Height(units) : fieldOrView;

    public static IFieldWithLabel Scale(this IFieldWithLabel fieldOrView, Scale scale) =>
        fieldOrView is Field f ? f.Scale(scale) : fieldOrView;

    public static IFieldWithLabel Small(this IFieldWithLabel fieldOrView) =>
        fieldOrView is Field f ? f.Small() : fieldOrView;

    public static IFieldWithLabel Medium(this IFieldWithLabel fieldOrView) =>
        fieldOrView is Field f ? f.Medium() : fieldOrView;

    public static IFieldWithLabel Large(this IFieldWithLabel fieldOrView) =>
        fieldOrView is Field f ? f.Large() : fieldOrView;

    public static IWidget WithTooltip(this IFieldWithLabel fieldOrView, string toolTip) =>
        new Tooltip(fieldOrView, toolTip);

    public static IFieldWithLabel WithField(this IAnyInput input)
    {
        if (input is TextInputBase tb && tb.BoundState != null &&
            tb.Variant is TextInputs.Email or TextInputs.Password or TextInputs.Tel or TextInputs.Url)
            return new ValidatedFieldView(tb, tb.BoundState, tb.Variant);
        return new Field(input);
    }
}