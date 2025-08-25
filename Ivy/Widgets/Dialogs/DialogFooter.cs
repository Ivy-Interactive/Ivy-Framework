using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents the footer section of a dialog widget.
/// </summary>
public record DialogFooter : WidgetBase<DialogFooter>
{
    /// <summary>
    /// Initializes a new instance of the DialogFooter class.
    /// </summary>
    /// <param name="children">
    /// The child widgets.
    /// </param>
    public DialogFooter(params object[] children) : base(children)
    {
    }
}