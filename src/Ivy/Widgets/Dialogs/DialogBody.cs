using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// The main content area of a Dialog.
/// </summary>
public record DialogBody : WidgetBase<DialogBody>
{
    public DialogBody(params object[] children) : base(children)
    {
    }

    internal DialogBody() { }
}