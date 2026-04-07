// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// The bottom area of a Dialog, typically used for action buttons.
/// </summary>
public record DialogFooter : WidgetBase<DialogFooter>
{
    public DialogFooter(params object[] children) : base(children)
    {
    }

    internal DialogFooter() { }
}