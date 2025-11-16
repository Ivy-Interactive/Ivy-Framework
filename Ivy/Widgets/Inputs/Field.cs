using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Views.Forms;
using Ivy.Widgets.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Ivy;
public class FieldView : ViewBase
{
    private readonly IAnyInput originalInput;
    private readonly string fieldName;
    private readonly TextInputs variant;
    private readonly List<Func<object?, (bool, string)>> validators = new();
    private string? label;
    private string? description;
    private bool required;
    private string? help;
    private Sizes size = Sizes.Medium;

    public FieldView(IAnyInput input, string fieldName, TextInputs variant)
    {
        this.originalInput = input;
        this.fieldName = fieldName;
        this.variant = variant;

        if (fieldName.EndsWith("Email", StringComparison.OrdinalIgnoreCase))
        {
            validators.Add(Validators.CreateEmailValidator(fieldName));
        }
    }

    public FieldView Label(string label) { this.label = label; return this; }
    public FieldView Description(string description) { this.description = description; return this; }
    public FieldView Required() { this.required = true; validators.Add(e => (Utils.IsValidRequired(e), "Required field")); return this; }
    public FieldView Help(string help) { this.help = help; return this; }
    public FieldView Size(Sizes size) { this.size = size; return this; }

    public override object? Build()
    {
        var invalidState = UseState((string?)null!);
        var blurOnceState = UseState(false);

        // Extract current value and create validation state
        var currentValue = originalInput is IInput<string> stringInput ? stringInput.Value : "";
        var inputState = Context.UseState(currentValue);

        // Get original OnChange to sync back to original state
        var originalOnChange = originalInput.GetType().GetProperty("OnChange")?.GetValue(originalInput)
            as Func<Event<IInput<string>, string>, ValueTask>;

        // Validate on blur
        UseEffect(() =>
        {
            if (blurOnceState.Value)
            {
                Validate(inputState.As<object>().Value, invalidState);
            }
        }, [inputState, blurOnceState]);

        // Create input with wrapped OnChange to sync both states
        var input = originalOnChange != null
            ? new TextInput<string>(
                inputState.As<string>().Value,
                async e =>
                {
                    inputState.As<string>().Set(e.Value);
                    await originalOnChange(e);
                },
                null,
                originalInput.Disabled,
                variant)
            : inputState.ToTextInput(disabled: originalInput.Disabled, variant: variant);

        return new Field(
            input.Invalid(invalidState.Value).HandleBlur((Event<IAnyInput> _) => { blurOnceState.Set(true); return ValueTask.CompletedTask; }).Size(size),
            label, description, required, help, fieldName)
        { Size = size };
    }

    private bool Validate<T>(T value, Core.Hooks.IState<string> invalid)
    {
        if (validators.Count == 0) return true;
        foreach (var validator in validators)
        {
            var (isValid, message) = validator(value);
            if (!isValid)
            {
                invalid.Set(message);
                return false;
            }
        }
        invalid.Set((string?)null!);
        return true;
    }
}

/// <summary>Wrapper widget providing structured layout and metadata for field input controls.</summary>
public record Field : WidgetBase<Field>
{
    /// <summary>Initializes Field instance.</summary>
    /// <param name="input">Input control.</param>
    /// <param name="label">Optional label text.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="required">Whether field is required.</param>
    /// <param name="help">Optional help text displayed as tooltip on info icon.</param>
    /// <param name="fieldName">Optional field name used for automatic validation detection (e.g., "Email", "Password").</param>
    public Field(IAnyInput input, string? label = null, string? description = null, bool required = false, string? help = null, string? fieldName = null) : base([input])
    {
        var labelProp = input.GetType().GetProperty("Label");
        if (labelProp != null && labelProp.PropertyType == typeof(string))
        {
            //Input handles label on its own
            var inputLabel = (string?)labelProp.GetValue(input);
            labelProp.SetValue(input, inputLabel ?? label);
            label = null;
        }

        var descriptionProp = input.GetType().GetProperty("Description");
        if (descriptionProp != null && descriptionProp.PropertyType == typeof(string))
        {
            //Input handles description on its own
            var inputDescription = (string?)descriptionProp.GetValue(input);
            descriptionProp.SetValue(input, inputDescription ?? description);
            description = null;
        }
        Label = label;
        Description = description;
        Required = required;
        Help = help;
        FieldName = fieldName;
    }

    /// <summary>Label text displayed for field.</summary>
    [Prop] public string? Label { get; set; }

    /// <summary>Description or help text displayed for field.</summary>
    [Prop] public string? Description { get; set; }

    /// <summary>Whether field is required. Default is false.</summary>
    [Prop] public bool Required { get; set; }

    /// <summary>Help text displayed as tooltip on info icon next to label.</summary>
    [Prop] public string? Help { get; set; }

    /// <summary>The size of the field affecting label and input sizing. Default is Medium.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    /// <summary>Optional field name used for automatic validation detection (e.g., "Email", "Password").</summary>
    [Prop] public string? FieldName { get; set; }

    /// <summary>Prevents adding children to Field widgets using pipe operator.</summary>
    /// <param name="widget">Field widget.</param>
    /// <param name="child">Child object attempting to be added.</param>
    /// <returns>Always throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">Field widgets wrap single input control.</exception>
    public static Field operator |(Field widget, object child)
    {
        throw new NotSupportedException("Field does not support children.");
    }
}

/// <summary>
/// Provides extension methods for creating and configuring field with fluent syntax.
/// </summary>
public static class FieldExtensions
{

    /// <summary>Sets the label text for the field view or field.</summary>
    public static object Label(this object view, string label) =>
        view is FieldView fv ? fv.Label(label) : view is Field f ? f with { Label = label } : view;

    /// <summary>Sets the description text for the field view or field.</summary>
    public static object Description(this object view, string description) =>
        view is FieldView fv ? fv.Description(description) : view is Field f ? f with { Description = description } : view;

    /// <summary>Sets the help text for the field view or field.</summary>
    public static object Help(this object view, string help) =>
        view is FieldView fv ? fv.Help(help) : view is Field f ? f with { Help = help } : view;

    /// <summary>Make the field view or field required.</summary>
    public static object Required(this object view) =>
        view is FieldView fv ? fv.Required() : view is Field f ? f with { Required = true } : view;

    /// <summary>Sets the size of the field affecting label and input sizing.</summary>
    /// <param name="field">The field to configure.</param>
    /// <param name="size">The size of the field (Small, Medium, Large).</param>
    public static Field Size(this Field field, Sizes size) => field with { Size = size };

    /// <summary>
    /// Wraps the specified input control in a <see cref="Field"/> widget.
    /// Automatically detects email and password inputs and applies validation.
    /// </summary>
    public static object WithField(this IAnyInput input)
    {
        if (input is IAnyTextInput textInput)
        {
            var fieldName = textInput.Variant == TextInputs.Email ? "Email"
                : textInput.Variant == TextInputs.Password ? "Password" : null;
            if (fieldName != null)
                return new FieldView(input, fieldName, textInput.Variant);
        }
        return new Field(input);
    }

}

