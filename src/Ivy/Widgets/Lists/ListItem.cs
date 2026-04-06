using Ivy.Core;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A single item within a List or SidebarMenu.
/// </summary>
public record ListItem : WidgetBase<ListItem>
{
    [OverloadResolutionPriority(1)]
    public ListItem(string? title = null, string? subtitle = null, Func<Event<ListItem>, ValueTask>? onClick = null, Icons? icon = null, object? badge = null, object? tag = null, object[]? items = null, bool disabled = false) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick.ToEventHandler();
        Disabled = disabled;
    }

    // Overload for Action<Event<ListItem>>
    public ListItem(string? title = null, string? subtitle = null, Action<Event<ListItem>>? onClick = null, Icons? icon = null, object? badge = null, object? tag = null, object[]? items = null, bool disabled = false) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick.ToEventHandler();
        Disabled = disabled;
    }

    // Overload for simple Action (no parameters)
    public ListItem(string? title = null, string? subtitle = null, Action? onClick = null, Icons? icon = null, object? badge = null, object? tag = null, object[]? items = null, bool disabled = false) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick == null ? null : new(_ => { onClick(); return ValueTask.CompletedTask; });
        Disabled = disabled;
    }

    internal ListItem()
    {
    }

    [Prop] public string? Title { get; set; }

    [Prop] public string? Subtitle { get; set; }

    [Prop] public Icons? Icon { get; set; }

    [Prop] public string? Badge { get; set; }

    public object? Tag { get; set; } //not a prop!

    [Prop] public bool Disabled { get; set; }

    [Event] public EventHandler<Event<ListItem>>? OnClick { get; set; }
}

public static class ListItemExtensions
{
    public static ListItem Content(this ListItem listItem, object child) => listItem with { Children = [child] };

    public static ListItem Disabled(this ListItem listItem, bool disabled = true) => listItem with { Disabled = disabled };

    public static ListItem Title(this ListItem listItem, string title) => listItem with { Title = title };

    public static ListItem Subtitle(this ListItem listItem, string subtitle) => listItem with { Subtitle = subtitle };

    public static ListItem Icon(this ListItem listItem, Icons icon) => listItem with { Icon = icon };

    public static ListItem Badge(this ListItem listItem, string badge) => listItem with { Badge = badge };

    public static ListItem Tag(this ListItem listItem, object tag) => listItem with { Tag = tag };

    public static ListItem OnClick(this ListItem listItem, EventHandler<Event<ListItem>> onClick) => listItem with { OnClick = onClick };

    public static ListItem OnClick(this ListItem listItem, Action<Event<ListItem>> onClick) => listItem with { OnClick = onClick.ToEventHandler() };

    public static ListItem OnClick(this ListItem listItem, Action onClick) => listItem with { OnClick = new(_ => { onClick(); return ValueTask.CompletedTask; }) };

    public static ListItem OnClick(this ListItem listItem, Func<Event<ListItem>, ValueTask> onClick) => listItem with { OnClick = onClick.ToEventHandler() };
}