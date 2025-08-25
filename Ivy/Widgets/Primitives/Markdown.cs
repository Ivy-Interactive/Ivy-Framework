using Ivy.Core;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A widget for rendering Markdown content with syntax highlighting, tables, math support, and interactive link handling.
/// </summary>
/// <remarks>
/// Supports standard Markdown features including formatting, lists, links, blockquotes, tables, code blocks with syntax highlighting,
/// and math expressions. Links can be handled with custom click events.
/// </remarks>
public record Markdown : WidgetBase<Markdown>
{
    /// <summary>
    /// Initializes a new Markdown widget with the specified content and optional link click handler.
    /// </summary>
    /// <param name="content">The Markdown content to render.</param>
    /// <param name="onLinkClick">Optional event handler for when links in the markdown are clicked.</param>
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

    /// <summary>Gets or sets the Markdown content to render.</summary>
    /// <value>The Markdown content string that will be rendered as formatted HTML.</value>
    [Prop] public string Content { get; set; }

    /// <summary>Gets or sets the event handler called when links in the markdown are clicked.</summary>
    /// <value>An event handler that receives the clicked link URL.</value>
    [Event] public Func<Event<Markdown, string>, ValueTask>? OnLinkClick { get; set; }
}

/// <summary>
/// Extension methods for configuring Markdown widgets.
/// </summary>
public static class MarkdownExtensions
{
    /// <summary>
    /// Sets the link click event handler for the Markdown widget.
    /// </summary>
    /// <param name="button">The Markdown widget to configure.</param>
    /// <param name="onLinkClick">The event handler that receives the full event context.</param>
    /// <returns>The Markdown widget with the specified link click handler.</returns>
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

    /// <summary>
    /// Sets the link click event handler for the Markdown widget with a simplified callback.
    /// </summary>
    /// <param name="button">The Markdown widget to configure.</param>
    /// <param name="onLinkClick">The simplified event handler that receives only the clicked link URL.</param>
    /// <returns>The Markdown widget with the specified link click handler.</returns>
    public static Markdown HandleLinkClick(this Markdown button, Action<string> onLinkClick)
    {
        return button with { OnLinkClick = @event => { onLinkClick(@event.Value); return ValueTask.CompletedTask; } };
    }
}