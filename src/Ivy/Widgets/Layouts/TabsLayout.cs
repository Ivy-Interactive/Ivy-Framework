using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum TabsVariant
{
    Tabs,
    Content
}

/// <summary>
/// Organizes content into separate views accessible via tabs.
/// </summary>
public record TabsLayout : WidgetBase<TabsLayout>
{
    [OverloadResolutionPriority(1)]
    public TabsLayout(Func<Event<TabsLayout, int>, ValueTask>? onSelect, Func<Event<TabsLayout, int>, ValueTask>? onClose, Func<Event<TabsLayout, int>, ValueTask>? onRefresh, Func<Event<TabsLayout, int[]>, ValueTask>? onReorder, int? selectedIndex, params Tab[] tabs) : base(tabs.Cast<object>().ToArray())
    {
        OnSelect = onSelect.ToEventHandler();
        OnClose = onClose.ToEventHandler();
        OnRefresh = onRefresh.ToEventHandler();
        OnReorder = onReorder.ToEventHandler();
        SelectedIndex = selectedIndex;
    }

    internal TabsLayout()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    public TabsLayout(Action<Event<TabsLayout, int>>? onSelect, Action<Event<TabsLayout, int>>? onClose,
        Action<Event<TabsLayout, int>>? onRefresh, Action<Event<TabsLayout, int[]>>? onReorder, int? selectedIndex,
        params Tab[] tabs)
        : this(
            onSelect?.ToValueTask(),
            onClose?.ToValueTask(),
            onRefresh?.ToValueTask(),
            onReorder?.ToValueTask(),
            selectedIndex, tabs)
    {
    }

    [Prop] public int? SelectedIndex { get; set; }

    [Prop] public TabsVariant Variant { get; set; } = TabsVariant.Content;

    [Prop] public bool RemoveParentPadding { get; set; } = false;

    [Prop] public Thickness? Padding { get; set; } = new Thickness(4);

    [Prop] public string? AddButtonText { get; set; }

    [Event] public EventHandler<Event<TabsLayout, int>>? OnSelect { get; set; }

    [Event] public EventHandler<Event<TabsLayout, int>>? OnClose { get; set; }

    [Event] public EventHandler<Event<TabsLayout, int>>? OnRefresh { get; set; }

    [Event] public EventHandler<Event<TabsLayout, int[]>>? OnReorder { get; set; }

    [Event] public EventHandler<Event<TabsLayout, int>>? OnAddButtonClick { get; set; }
}

public static class TabsLayoutExtensions
{
    public static TabsLayout Variant(this TabsLayout tabsLayout, TabsVariant variant)
    {
        return tabsLayout with { Variant = variant };
    }

    public static TabsLayout RemoveParentPadding(this TabsLayout tabsLayout, bool removeParentPadding = true)
    {
        return tabsLayout with { RemoveParentPadding = removeParentPadding };
    }

    public static TabsLayout Padding(this TabsLayout tabsLayout, Thickness? padding)
    {
        return tabsLayout with { Padding = padding };
    }

    public static TabsLayout Padding(this TabsLayout tabsLayout, int padding)
    {
        return tabsLayout with { Padding = new Thickness(padding) };
    }

    public static TabsLayout Padding(this TabsLayout tabsLayout, int verticalPadding, int horizontalPadding)
    {
        return tabsLayout with { Padding = new Thickness(horizontalPadding, verticalPadding) };
    }

    public static TabsLayout Padding(this TabsLayout tabsLayout, int left, int top, int right, int bottom)
    {
        return tabsLayout with { Padding = new Thickness(left, top, right, bottom) };
    }

    public static TabsLayout AddButton(this TabsLayout tabsLayout, string? addButtonText, Func<Event<TabsLayout, int>, ValueTask>? onAddButtonClick = null)
    {
        return tabsLayout with { AddButtonText = addButtonText, OnAddButtonClick = onAddButtonClick.ToEventHandler() };
    }

    public static TabsLayout AddButton(this TabsLayout tabsLayout, string? addButtonText, Action<Event<TabsLayout, int>>? onAddButtonClick)
    {
        return tabsLayout with
        {
            AddButtonText = addButtonText,
            OnAddButtonClick = onAddButtonClick.ToEventHandler()
        };
    }
}

/// <summary>
/// A single tab item within a TabsLayout.
/// </summary>
public record Tab : WidgetBase<Tab>
{
    public Tab(string title, object? content = null) : base(content != null ? [content] : [])
    {
        Title = title;
    }

    [Prop] public string Title { get; set; }

    [Prop] public Icons? Icon { get; set; }

    [Prop] public string? Badge { get; set; }
}

public static class TabExtensions
{
    public static Tab Icon(this Tab tab, Icons? icon)
    {
        return tab with { Icon = icon };
    }

    public static Tab Badge(this Tab tab, string badge)
    {
        return tab with { Badge = badge };
    }
}
