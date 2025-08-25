using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents the main content area of a dialog widget.
/// </summary>
public record DialogBody : WidgetBase<DialogBody>
{
    /// <summary>
    /// Initializes a new instance of the DialogBody class.
    /// </summary>
    /// <param name="children">
    /// The child widgets.
    /// </param>
    public DialogBody(params object[] children) : base(children)
    {
    }
}