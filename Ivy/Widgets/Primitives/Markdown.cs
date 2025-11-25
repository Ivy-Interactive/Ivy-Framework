using Ivy.Core;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Markdown : WidgetBase<Markdown>
{
    [OverloadResolutionPriority(1)]
    public Markdown(string content, Func<Event<Markdown, string>, ValueTask>? onLinkClick = null)
    {
        Content = content;
        OnLinkClick = onLinkClick;
    }

    // Overload for Action<Event<Markdown, string>>
    public Markdown(string content, Action<Event<Markdown, string>>? onLinkClick = null)
    {
        Content = content;
        OnLinkClick = onLinkClick?.ToValueTask();
    }

    [Prop] public string Content { get; set; }

    [Event] public Func<Event<Markdown, string>, ValueTask>? OnLinkClick { get; set; }
}

public static class MarkdownExtensions
{
    [OverloadResolutionPriority(1)]
    public static Markdown HandleLinkClick(this Markdown button, Func<Event<Markdown, string>, ValueTask> onLinkClick)
    {
        return button with { OnLinkClick = onLinkClick };
    }

    // Overload for Action<Event<Markdown, string>>
    public static Markdown HandleLinkClick(this Markdown button, Action<Event<Markdown, string>> onLinkClick)
    {
        return button with { OnLinkClick = onLinkClick.ToValueTask() };
    }

    public static Markdown HandleLinkClick(this Markdown button, Action<string> onLinkClick)
    {
        return button with { OnLinkClick = @event => { onLinkClick(@event.Value); return ValueTask.CompletedTask; } };
    }
}