using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// The top area of a Dialog containing the title and close button.
/// </summary>
public record DialogHeader : WidgetBase<DialogHeader>
{
    public DialogHeader(string title)
    {
        Title = title;
    }

    internal DialogHeader() { }

    [Prop]
    public string? Title { get; set; }

    public static DialogHeader operator |(DialogHeader widget, object child)
    {
        throw new NotSupportedException("DialogHeader does not support children.");
    }
}