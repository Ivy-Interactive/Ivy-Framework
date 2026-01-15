using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// The footer section of a Dialog, usually containing action buttons.
/// </summary>
public record DialogFooter : WidgetBase<DialogFooter>
{
    public DialogFooter(params object[] children) : base(children)
    {
    }

    internal DialogFooter() { }
}