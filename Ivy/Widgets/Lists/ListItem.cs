using Ivy.Core;
using Ivy.Shared;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Structured list item widget providing consistent layout for displaying information with title, subtitle, icon, badge, and click interactions.</summary>
public record ListItem : WidgetBase<ListItem>
{
    /// <summary>Initializes ListItem with comprehensive configuration options.</summary>
    /// <param name="title">Primary text displayed as main content of list item.</param>
    /// <param name="subtitle">Optional secondary text displayed below title for additional context.</param>
    /// <param name="onClick">Optional click event handler for making list item interactive.</param>
    /// <param name="icon">Optional icon displayed alongside title for visual identification.</param>
    /// <param name="badge">Optional badge content (converted to string) displayed as visual indicator or count.</param>
    /// <param name="tag">Optional tag object for storing additional data (not serialized).</param>
    /// <param name="items">Optional child items or widgets to display within list item for nested content.</param>
    [OverloadResolutionPriority(1)]
    public ListItem(string? title = null, string? subtitle = null, Func<Event<ListItem>, ValueTask>? onClick = null, Icons? icon = Icons.None, object? badge = null, object? tag = null, object[]? items = null) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick;
    }

    // Overload for Action<Event<ListItem>>
    public ListItem(string? title = null, string? subtitle = null, Action<Event<ListItem>>? onClick = null, Icons? icon = Icons.None, object? badge = null, object? tag = null, object[]? items = null) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick?.ToValueTask();
    }

    // Overload for simple Action (no parameters)
    public ListItem(string? title = null, string? subtitle = null, Action? onClick = null, Icons? icon = Icons.None, object? badge = null, object? tag = null, object[]? items = null) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick == null ? null : (_ => { onClick(); return ValueTask.CompletedTask; });
    }

    /// <summary>Primary text displayed as main content of list item.</summary>
    [Prop] public string? Title { get; }

    /// <summary>Secondary text displayed below title for additional context.</summary>
    [Prop] public string? Subtitle { get; }

    /// <summary>Icon displayed alongside title for visual identification.</summary>
    [Prop] public Icons? Icon { get; }

    /// <summary>Badge content displayed as visual indicator or count, automatically converted to string.</summary>
    [Prop] public string? Badge { get; set; }

    /// <summary>Tag object for storing additional data associated with list item (not serialized).</summary>
    public object? Tag { get; } //not a prop!

    /// <summary>Event handler called when list item is clicked.</summary>
    [Event] public Func<Event<ListItem>, ValueTask>? OnClick { get; set; }
}