using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents the header section of a dialog widget.
/// </summary>
public record DialogHeader : WidgetBase<DialogHeader>
{
    /// <summary>
    /// Initializes a new instance of the DialogHeader class.
    /// </summary>
    /// <param name="title">
    /// The title text.
    /// </param>
    public DialogHeader(string title)
    {
        Title = title;
    }

    /// <summary>
    /// Gets or sets the title text displayed in the dialog header.
    /// </summary>
    /// <value>
    /// A string containing the dialog title.
    /// </value>
    [Prop]
    public string Title { get; set; }

    /// <summary>
    /// Overrides the | operator to prevent adding children to DialogHeader widgets.
    /// </summary>
    /// <param name="widget">The DialogHeader widget.</param>
    /// <param name="child">The child object.</param>
    /// <returns>This method always throws an exception.</returns>
    /// <exception cref="NotSupportedException">
    /// Always thrown because DialogHeader widgets only display title text.
    /// </exception>
    public static DialogHeader operator |(DialogHeader widget, object child)
    {
        throw new NotSupportedException("DialogHeader does not support children.");
    }
}