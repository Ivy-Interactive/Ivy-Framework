using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record ListItem : WidgetBase<ListItem>
{
    public ListItem(string? title = null, string? subtitle = null, Action<Event<ListItem>>? onClick = null, Icons? icon = Icons.None, object? badge = null, object? tag = null, object[]? items = null) : base(items ?? [])
    {
        Title = title;
        Subtitle = subtitle;
        Icon = icon;
        Badge = badge?.ToString();
        Tag = tag;
        OnClick = onClick;
    }

    [Prop] public string? Title { get; }
    [Prop] public string? Subtitle { get; }
    [Prop] public Icons? Icon { get; }
    [Prop] public string? Badge { get; set; }

    public object? Tag { get; } //not a prop!

    [Event] public Action<Event<ListItem>>? OnClick { get; set; }
}