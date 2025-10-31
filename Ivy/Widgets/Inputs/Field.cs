using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ivy;

/// <summary>Interface for field-like objects that support Label, Description, Required, and Size methods.</summary>
public interface IFieldLike
{
    string? Label { get; set; }
    string? Description { get; set; }
    bool Required { get; set; }
    Sizes Size { get; set; }
}

/// <summary>Wrapper widget providing structured layout and metadata for field input controls.</summary>
public record Field : WidgetBase<Field>, IFieldLike
{
    /// <summary>Initializes Field instance.</summary>
    /// <param name="input">Input control.</param>
    /// <param name="label">Optional label text.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="required">Whether field is required.</param>
    public Field(IAnyInput input, string? label = null, string? description = null, bool required = false) : base([input])
    {
        // Store the input for potential validation
        _input = input;
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
    }

    /// <summary>Label text displayed for field.</summary>
    [Prop] public string? Label { get; set; }

    /// <summary>Description or help text displayed for field.</summary>
    [Prop] public string? Description { get; set; }

    /// <summary>Whether field is required. Default is false.</summary>
    [Prop] public bool Required { get; set; }

    /// <summary>The size of the field affecting label and input sizing. Default is Medium.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    private readonly IAnyInput _input;

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

    /// <summary>Sets the label text for the child input.</summary>
    /// <param name="field">The field to configure.</param>
    /// <param name="label">The label text to display for the child input.</param>
    public static Field Label(this Field field, string label) => field with { Label = label };

    /// <summary>Sets the label text for a FieldView (used for validation-enabled fields).</summary>
    /// <param name="fieldView">The field view to configure.</param>
    /// <param name="label">The label text to display for the child input.</param>
    public static FieldView Label(this FieldView fieldView, string label)
    {
        fieldView.Label = label;
        return fieldView;
    }

    /// <summary>Sets the label text for any field-like object.</summary>
    /// <param name="fieldLike">The field-like object to configure.</param>
    /// <param name="label">The label text to display for the child input.</param>
    public static IFieldLike Label(this IFieldLike fieldLike, string label)
    {
        fieldLike.Label = label;
        return fieldLike;
    }

    /// <summary>Sets the description text for the child input.</summary>
    /// <param name="field">The field to configure.</param>
    /// <param name="description">The description text to display for the child input.</param>
    public static Field Description(this Field field, string description) => field with { Description = description };

    /// <summary>Sets the description text for a FieldView (used for validation-enabled fields).</summary>
    /// <param name="fieldView">The field view to configure.</param>
    /// <param name="description">The description text to display for the child input.</param>
    public static FieldView Description(this FieldView fieldView, string description)
    {
        fieldView.Description = description;
        return fieldView;
    }

    /// <summary>Sets the description text for any field-like object.</summary>
    /// <param name="fieldLike">The field-like object to configure.</param>
    /// <param name="description">The description text to display for the child input.</param>
    public static IFieldLike Description(this IFieldLike fieldLike, string description)
    {
        fieldLike.Description = description;
        return fieldLike;
    }


    /// <summary>Make the input child required</summary>
    /// <param name="field">The field to configure.</param>
    public static Field Required(this Field field) => field with { Required = true };

    /// <summary>Make the input child required for a FieldView (used for validation-enabled fields).</summary>
    /// <param name="fieldView">The field view to configure.</param>
    public static FieldView Required(this FieldView fieldView)
    {
        fieldView.Required = true;
        return fieldView;
    }

    /// <summary>Make the input child required for any field-like object.</summary>
    /// <param name="fieldLike">The field-like object to configure.</param>
    public static IFieldLike Required(this IFieldLike fieldLike)
    {
        fieldLike.Required = true;
        return fieldLike;
    }

    /// <summary>Sets the size of the field affecting label and input sizing.</summary>
    /// <param name="field">The field to configure.</param>
    /// <param name="size">The size of the field (Small, Medium, Large).</param>
    public static Field Size(this Field field, Sizes size) => field with { Size = size };

    /// <summary>Sets the size of the field for a FieldView (used for validation-enabled fields).</summary>
    /// <param name="fieldView">The field view to configure.</param>
    /// <param name="size">The size of the field (Small, Medium, Large).</param>
    public static FieldView Size(this FieldView fieldView, Sizes size)
    {
        fieldView.Size = size;
        return fieldView;
    }

    /// <summary>Sets the size of the field for any field-like object.</summary>
    /// <param name="fieldLike">The field-like object to configure.</param>
    /// <param name="size">The size of the field (Small, Medium, Large).</param>
    public static IFieldLike Size(this IFieldLike fieldLike, Sizes size)
    {
        fieldLike.Size = size;
        return fieldLike;
    }

    /// <summary>
    /// Wraps the specified input control in a <see cref="Field"/> widget.
    /// Automatically adds validation for Email and Password inputs when used in a ViewBase context.
    /// </summary>
    /// <param name="input">The input control to wrap.</param>
    /// <returns>A field-like object (Field or FieldView) containing the input control.</returns>
    public static IFieldLike WithField(this IAnyInput input)
    {
        // Check if this is an Email or Password input with a bound state
        if (input is TextInputBase textInput && textInput.BoundState != null)
        {
            // If it's Email or Password, return a FieldView that handles validation
            if (textInput.Variant == TextInputs.Email || textInput.Variant == TextInputs.Password)
            {
                // Return a FieldView that will handle validation when built
                return new FieldView(textInput.BoundState, textInput);
            }
        }

        return new Field(input);
    }

    /// <summary>
    /// View that wraps a field with automatic validation for Email and Password inputs.
    /// Supports Field extension methods for method chaining.
    /// </summary>
    public class FieldView : ViewBase, IFieldLike
    {
        private readonly IAnyState _state;
        private readonly TextInputBase _input;
        public string? Label { get; set; }
        public string? Description { get; set; }
        public bool Required { get; set; }
        public Sizes Size { get; set; } = Sizes.Medium;

        public FieldView(IAnyState state, TextInputBase input)
        {
            _state = state;
            _input = input;
        }

        public override object? Build()
        {
            TextInputBase validatedInput;

            if (_input.Variant == TextInputs.Email)
            {
                validatedInput = _state.WithEmailValidation(this, _input);
            }
            else if (_input.Variant == TextInputs.Password)
            {
                validatedInput = _state.WithPasswordValidation(this, _input);
            }
            else
            {
                validatedInput = _input;
            }

            var field = new Field(validatedInput, Label, Description, Required) { Size = Size };
            return field;
        }
    }

    /// <summary>
    /// Wraps the specified input control in a <see cref="Field"/> widget with automatic validation for Email and Password inputs.
    /// </summary>
    /// <param name="state">The state object the input is bound to.</param>
    /// <param name="view">The ViewBase context for validation.</param>
    /// <param name="input">The input control to wrap.</param>
    /// <returns>A <see cref="Field"/> widget containing the input control with validation if applicable.</returns>
    public static Field WithField(this IAnyState state, ViewBase view, IAnyInput input)
    {
        // Add automatic validation for Email and Password inputs
        if (input is TextInputBase textInput)
        {
            if (textInput.Variant == TextInputs.Email)
            {
                var validatedInput = state.WithEmailValidation(view, textInput);
                return new Field(validatedInput);
            }
            else if (textInput.Variant == TextInputs.Password)
            {
                var validatedInput = state.WithPasswordValidation(view, textInput);
                return new Field(validatedInput);
            }
        }

        return new Field(input);
    }

}

