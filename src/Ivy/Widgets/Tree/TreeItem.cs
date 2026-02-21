using Ivy.Core;
using Ivy.Shared;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ivy;

public record TreeItem : WidgetBase<TreeItem>
{
    [OverloadResolutionPriority(1)]
    public TreeItem(
        string label,
        Icons? icon = null,
        bool open = false,
        Func<Event<TreeItem>, ValueTask>? onClick = null,
        object[]? items = null) : base(items ?? [])
    {
        Label = label;
        Icon = icon;
        Open = open;
        OnClick = onClick;
    }

    public TreeItem(
        string label,
        Icons? icon = null,
        bool open = false,
        Action<Event<TreeItem>>? onClick = null,
        object[]? items = null) : base(items ?? [])
    {
        Label = label;
        Icon = icon;
        Open = open;
        OnClick = onClick?.ToValueTask();
    }

    public TreeItem(
        string label,
        Icons? icon = null,
        bool open = false,
        Action? onClick = null,
        object[]? items = null) : base(items ?? [])
    {
        Label = label;
        Icon = icon;
        Open = open;
        OnClick = onClick == null ? null : (_ => { onClick(); return ValueTask.CompletedTask; });
    }

    internal TreeItem()
    {
    }

    [Prop] public string? Label { get; set; }

    [Prop] public Icons? Icon { get; set; }

    [Prop] public bool Open { get; set; } = false;

    [Prop] public bool Disabled { get; set; } = false;

    [Event] public Func<Event<TreeItem>, ValueTask>? OnClick { get; set; }
}

public static class TreeItemExtensions
{
    public static TreeItem Open(this TreeItem item, bool open = true)
    {
        item.Open = open;
        return item;
    }

    public static TreeItem Disabled(this TreeItem item, bool disabled = true)
    {
        item.Disabled = disabled;
        return item;
    }

    public static TreeItem HandleClick(this TreeItem item, Func<Event<TreeItem>, ValueTask> onClick)
        => item with { OnClick = onClick };

    public static TreeItem HandleClick(this TreeItem item, Action<Event<TreeItem>> onClick)
        => item with { OnClick = onClick.ToValueTask() };

    public static TreeItem HandleClick(this TreeItem item, Action onClick)
        => item with { OnClick = _ => { onClick(); return ValueTask.CompletedTask; } };
}
