using Ivy.Core;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Widget for rendering Markdown content with syntax highlighting, tables, math support, and interactive link handling.</summary>
public record Markdown : WidgetBase<Markdown>
{
    /// <summary>Initializes Markdown widget with specified content and optional link click handler.</summary>
    /// <param name="content">Markdown content to render.</param>
    /// <param name="onLinkClick">Optional event handler for when links in markdown are clicked.</param>
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

    /// <summary>Markdown content to render as formatted HTML.</summary>
    [Prop] public string Content { get; set; }

    /// <summary>Event handler called when links in markdown are clicked.</summary>
    [Event] public Func<Event<Markdown, string>, ValueTask>? OnLinkClick { get; set; }
}

/// <summary>Extension methods for configuring Markdown widgets.</summary>
public static class MarkdownExtensions
{
    /// <summary>Sets link click event handler for Markdown widget.</summary>
    /// <param name="button">Markdown widget to configure.</param>
    /// <param name="onLinkClick">Event handler receiving full event context.</param>
    /// <returns>Markdown widget with specified link click handler.</returns>
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

    /// <summary>Sets link click event handler for Markdown widget with simplified callback.</summary>
    /// <param name="button">Markdown widget to configure.</param>
    /// <param name="onLinkClick">Simplified event handler receiving only clicked link URL.</param>
    /// <returns>Markdown widget with specified link click handler.</returns>
    public static Markdown HandleLinkClick(this Markdown button, Action<string> onLinkClick)
    {
        return button with { OnLinkClick = @event => { onLinkClick(@event.Value); return ValueTask.CompletedTask; } };
    }
}