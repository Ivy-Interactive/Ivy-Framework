using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record DialogHeader : WidgetBase<DialogHeader>
{
    public DialogHeader(string title)
    {
        Title = title;
    }

    [Prop]
    public string Title { get; set; }

    public static DialogHeader operator |(DialogHeader widget, object child)
    {
        throw new NotSupportedException("DialogHeader does not support children.");
    }
}