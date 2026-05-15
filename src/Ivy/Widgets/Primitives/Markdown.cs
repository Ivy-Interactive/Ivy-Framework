using Ivy.Core;
using System.Runtime.CompilerServices;

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

    [Prop] public bool DangerouslyAllowLocalFiles { get; set; }

    /// <summary>
    /// Applies article-grade typography (heading top-margins, h2 divider, relaxed
    /// body line-height) so standalone markdown matches the spacing used inside
    /// the <see cref="Ivy.Article"/> widget without pulling in its TOC/footer chrome.
    /// </summary>
    [Prop] public bool Article { get; set; }

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

    public static Markdown DangerouslyAllowLocalFiles(this Markdown markdown, bool allow = true)
    {
        return markdown with { DangerouslyAllowLocalFiles = allow };
    }

    /// <summary>
    /// Renders with article-grade typography so standalone markdown matches the
    /// heading spacing and h2 divider used inside the <see cref="Ivy.Article"/>
    /// widget, without its TOC/footer chrome.
    /// </summary>
    public static Markdown Article(this Markdown markdown, bool article = true)
    {
        return markdown with { Article = article };
    }
}