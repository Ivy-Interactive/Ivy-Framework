using Ivy.Core;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Wrapper widget providing structured layout and metadata for form input controls.</summary>
public record FormField : WidgetBase<FormField>
{
    /// <summary>Initializes FormField instance.</summary>
    /// <param name="input">Input control.</param>
    /// <param name="label">Optional label text.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="required">Whether form field is required.</param>
    internal FormField(IAnyInput input, string? label = null, string? description = null, bool required = false) : base([input])
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

    /// <summary>Label text displayed for form field.</summary>
    [Prop] public string? Label { get; set; }

    /// <summary>Description or help text displayed for form field.</summary>
    [Prop] public string? Description { get; set; }

    /// <summary>Whether form field is required for form submission. Default is false.</summary>
    [Prop] public bool Required { get; set; }

    /// <summary>Prevents adding children to FormField widgets using pipe operator.</summary>
    /// <param name="widget">FormField widget.</param>
    /// <param name="child">Child object attempting to be added.</param>
    /// <returns>Always throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">FormField widgets wrap single input control.</exception>
    public static FormField operator |(FormField widget, object child)
    {
        throw new NotSupportedException("FormField does not support children.");
    }
}