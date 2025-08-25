using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a single label-value pair in a structured data display. Detail widgets are typically used
/// to show object properties or model fields in a formatted, readable way. They are commonly generated
/// automatically from objects using the ToDetails() extension method.
/// </summary>
public record Detail : WidgetBase<Detail>
{
    /// <summary>
    /// Initializes a new instance of the Detail class with the specified label, value, and formatting options.
    /// </summary>
    /// <param name="label">
    /// The label text to display for this detail item. If null, no label is shown.
    /// Typically represents the property or field name being displayed.
    /// </param>
    /// <param name="value">
    /// The value to display for this detail item. Can be any object that will be formatted
    /// according to the framework's content builders. If null, an empty detail is created.
    /// </param>
    /// <param name="multiLine">
    /// Indicates whether the value should be displayed in a multi-line format.
    /// When true, the value is rendered with line breaks and expanded spacing.
    /// When false, the value is displayed in a compact, single-line format.
    /// </param>
    public Detail(string? label, object? value, bool multiLine) : base(value != null ? [value] : [])
    {
        Label = label;
        MultiLine = multiLine;
    }

    /// <summary>
    /// Gets or sets the label text displayed for this detail item.
    /// </summary>
    [Prop] public string? Label { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the detail value should be displayed in multi-line format.
    /// </summary>
    [Prop] public bool MultiLine { get; set; }

    /// <summary>
    /// Overrides the | operator to prevent adding children to Detail widgets.
    /// </summary>
    /// <returns>This method always throws an exception.</returns>
    public static Detail operator |(Detail widget, object child)
    {
        throw new NotSupportedException("Detail does not support children.");
    }
}