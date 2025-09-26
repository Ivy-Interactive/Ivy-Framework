using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Single label-value pair in structured data display for showing object properties or model fields.</summary>
public record Detail : WidgetBase<Detail>
{
    /// <summary>Initializes Detail with specified label, value, and formatting options.</summary>
    /// <param name="label">Label text to display. If null, no label shown.</param>
    /// <param name="value">Value to display. If null, empty detail created.</param>
    /// <param name="multiLine">Whether value should be displayed in multi-line format.</param>
    public Detail(string? label, object? value, bool multiLine) : base(value != null ? [value] : [])
    {
        Label = label;
        MultiLine = multiLine;
    }

    /// <summary>Label text displayed for this detail item.</summary>
    [Prop] public string? Label { get; set; }

    /// <summary>Whether detail value should be displayed in multi-line format.</summary>
    [Prop] public bool MultiLine { get; set; }

    /// <summary>Prevents adding children to Detail widgets using pipe operator.</summary>
    /// <returns>Always throws NotSupportedException.</returns>
    public static Detail operator |(Detail widget, object child)
    {
        throw new NotSupportedException("Detail does not support children.");
    }
}