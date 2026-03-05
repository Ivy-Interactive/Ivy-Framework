using Ivy.Core;
using Ivy.Shared;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A single item within a List or SidebarMenu.
/// </summary>
public record ListItem : WidgetBase<ListItem>
{
    [OverloadResolutionPriority(1)]
    public ListItem(string? title = null, string? subtitle = null, Func<Event<ListItem>, ValueTask>? onClick = null, Icons? icon = null, object? badge = null, object? tag = null, object[]? items = null) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick.ToEventHandler();
    }

    // Overload for Action<Event<ListItem>>
    public ListItem(string? title = null, string? subtitle = null, Action<Event<ListItem>>? onClick = null, Icons? icon = null, object? badge = null, object? tag = null, object[]? items = null) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick.ToEventHandler();
    }

    // Overload for simple Action (no parameters)
    public ListItem(string? title = null, string? subtitle = null, Action? onClick = null, Icons? icon = null, object? badge = null, object? tag = null, object[]? items = null) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick == null ? null : new(_ => { onClick(); return ValueTask.CompletedTask; });
    }

    internal ListItem()
    {
    }

    [Prop] public string? Title { get; }

    [Prop] public string? Subtitle { get; }

    [Prop] public Icons? Icon { get; }

    [Prop] public string? Badge { get; set; }

    public object? Tag { get; } //not a prop!

    [Event] public EventHandler<Event<ListItem>>? OnClick { get; set; }
}

public static class ListItemExtensions
{
    public static ListItem Content(this ListItem listItem, object child) => listItem with { Children = [child] };
}