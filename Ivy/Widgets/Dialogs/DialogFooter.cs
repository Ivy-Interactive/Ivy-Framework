using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record DialogFooter : WidgetBase<DialogFooter>
{
    public DialogFooter(params object[] children) : base(children)
    {
    }
}