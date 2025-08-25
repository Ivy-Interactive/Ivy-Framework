using Ivy.Core;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Wrapper widget that provides structured layout and metadata for form input controls.
/// </summary>
public record FormField : WidgetBase<FormField>
{
    /// <summary>
    /// Initializes a new instance of the FormField class.
    /// </summary>
    /// <param name="input">
    /// The input control.
    /// </param>
    /// <param name="label">
    /// The optional label text.
    /// </param>
    /// <param name="description">
    /// The optional description.
    /// </param>
    /// <param name="required">
    /// Indicates whether this form field is required.
    /// </param>
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

    /// <summary>
    /// Gets or sets the label text displayed for this form field.
    /// </summary>
    /// <value>The label text, or null if no additional label should be displayed.</value>
    [Prop] public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the description or help text displayed for this form field.
    /// </summary>
    /// <value>
    /// The description text, or null if no additional description should be displayed.
    /// </value>
    [Prop] public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this form field is required for form submission.
    /// </summary>
    /// <value>
    /// true if the field is required for form submission; false if the field is optional.
    /// Default is false.
    /// </value>
    [Prop] public bool Required { get; set; }

    /// <summary>
    /// Overrides the | operator to prevent adding children to FormField widgets.
    /// </summary>
    /// <param name="widget">The FormField widget.</param>
    /// <param name="child">The child object attempting to be added.</param>
    /// <returns>This method always throws an exception.</returns>
    /// <exception cref="NotSupportedException">
    /// Always thrown because FormField widgets wrap a single input control.
    /// </exception>
    public static FormField operator |(FormField widget, object child)
    {
        throw new NotSupportedException("FormField does not support children.");
    }
}