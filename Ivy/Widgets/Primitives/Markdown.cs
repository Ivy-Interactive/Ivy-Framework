using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Markdown : WidgetBase<Markdown>
{
    public Markdown(string content, Action<Event<Markdown, string>>? onLinkClick = null)
    {
        Content = content;
        OnLinkClick = onLinkClick;
    }

    [Prop] public string Content { get; set; }
    [Event] public Action<Event<Markdown, string>>? OnLinkClick { get; set; }
}

public static class MarkdownExtensions
{
    public static Markdown HandleLinkClick(this Markdown button, Action<Event<Markdown, string>> onLinkClick)
    {
        return button with { OnLinkClick = onLinkClick };
    }

    public static Markdown HandleLinkClick(this Markdown button, Action<string> onLinkClick)
    {
        return button with { OnLinkClick = @event => onLinkClick(@event.Value) };
    }
}