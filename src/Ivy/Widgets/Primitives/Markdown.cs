using Ivy.Core;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Renders markdown content.
/// </summary>
public record Markdown : WidgetBase<Markdown>
{
    [OverloadResolutionPriority(1)]
    public Markdown(string content, Func<Event<Markdown, string>, ValueTask>? onLinkClick = null)
    {
        Content = content;
        OnLinkClick = onLinkClick.ToEventHandler();
    }

    // Overload for Action<Event<Markdown, string>>
    public Markdown(string content, Action<Event<Markdown, string>>? onLinkClick = null)
    {
        Content = content;
        OnLinkClick = onLinkClick.ToEventHandler();
    }

    internal Markdown() { }

    [Prop] public string Content { get; set; } = string.Empty;

    [Prop] public TextAlignment? TextAlignment { get; set; }

    [Event] public EventHandler<Event<Markdown, string>>? OnLinkClick { get; set; }
}

public static class MarkdownExtensions
{
    [OverloadResolutionPriority(1)]
    public static Markdown OnLinkClick(this Markdown button, Func<Event<Markdown, string>, ValueTask> onLinkClick)
    {
        return button with { OnLinkClick = new(onLinkClick) };
    }

    // Overload for Action<Event<Markdown, string>>
    public static Markdown OnLinkClick(this Markdown button, Action<Event<Markdown, string>> onLinkClick)
    {
        return button with { OnLinkClick = new(onLinkClick.ToValueTask()) };
    }

    public static Markdown OnLinkClick(this Markdown button, Action<string> onLinkClick)
    {
        return button with { OnLinkClick = new(@event => { onLinkClick(@event.Value); return ValueTask.CompletedTask; }) };
    }

    public static Markdown Align(this Markdown markdown, TextAlignment textAlignment)
    {
        return markdown with { TextAlignment = textAlignment };
    }

    public static Markdown Right(this Markdown markdown)
    {
        return markdown with { TextAlignment = TextAlignment.Right };
    }

    public static Markdown Left(this Markdown markdown)
    {
        return markdown with { TextAlignment = TextAlignment.Left };
    }

    public static Markdown Center(this Markdown markdown)
    {
        return markdown with { TextAlignment = TextAlignment.Center };
    }

    public static Markdown Justify(this Markdown markdown)
    {
        return markdown with { TextAlignment = TextAlignment.Justify };
    }
}