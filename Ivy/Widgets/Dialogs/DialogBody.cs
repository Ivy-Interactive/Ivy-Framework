using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record DialogBody : WidgetBase<DialogBody>
{
    public DialogBody(params object[] children) : base(children)
    {
    }
}