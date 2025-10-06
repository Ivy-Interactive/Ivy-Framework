using Ivy.Core;
using Ivy.Widgets.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ivy;

/// <summary>Wrapper widget providing structured layout and metadata for field input controls.</summary>
public record Field : WidgetBase<Field>
{
    /// <summary>Initializes Field instance.</summary>
    /// <param name="input">Input control.</param>
    /// <param name="label">Optional label text.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="required">Whether field is required.</param>
    public Field(IAnyInput input, string? label = null, string? description = null, bool required = false) : base([input])
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
    }

    /// <summary>Label text displayed for field.</summary>
    [Prop] public string? Label { get; set; }

    /// <summary>Description or help text displayed for field.</summary>
    [Prop] public string? Description { get; set; }

    /// <summary>Whether field is required. Default is false.</summary>
    [Prop] public bool Required { get; set; }

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

    /// <summary>Sets the description text for the child input.</summary>
    /// <param name="field">The field to configure.</param>
    /// <param name="description">The description text to display for the child input.</param>
    public static Field Description(this Field field, string description) => field with { Description = description };


    /// <summary>Make the input child required</summary>
    /// <param name="field">The field to configure.</param>
    public static Field Required(this Field field) => field with { Required = true };

    /// <summary>
    /// Wraps the specified input control in a <see cref="Field"/> widget.
    /// </summary>
    /// <param name="input">The input control to wrap.</param>
    /// <returns>A <see cref="Field"/> widget containing the input control.</returns>
    public static Field WithField(this IAnyInput input) => new Field(input);

}

