using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the visual style variant for tabbed interfaces.
/// </summary>
public enum TabsVariant
{
    /// <summary>Traditional tab interface with clickable tab buttons and an underline indicator for the active tab.</summary>
    Tabs,
    /// <summary>Content-focused variant with subtle tab indicators, emphasizing the displayed content over the tab controls.</summary>
    Content
}

/// <summary>
/// Represents a tabbed layout widget.
/// </summary>
public record TabsLayout : WidgetBase<TabsLayout>
{
    /// <summary>
    /// Initializes a new instance of the TabsLayout class.
    /// </summary>
    /// <param name="onSelect">Optional event handler for tab selection events.</param>
    /// <param name="onClose">Optional event handler for tab close events.</param>
    /// <param name="onRefresh">Optional event handler for tab refresh events.</param>
    /// <param name="onReorder">Optional event handler for tab reordering events.</param>
    /// <param name="selectedIndex">The index of the initially selected tab.</param>
    /// <param name="tabs">Variable number of Tab objects defining the tab structure.</param>
    [OverloadResolutionPriority(1)]
    public TabsLayout(Func<Event<TabsLayout, int>, ValueTask>? onSelect, Func<Event<TabsLayout, int>, ValueTask>? onClose, Func<Event<TabsLayout, int>, ValueTask>? onRefresh, Func<Event<TabsLayout, int[]>, ValueTask>? onReorder, int? selectedIndex, params Tab[] tabs) : base(tabs.Cast<object>().ToArray())
    {
        OnSelect = onSelect;
        OnClose = onClose;
        OnRefresh = onRefresh;
        OnReorder = onReorder;
        SelectedIndex = selectedIndex;
        Width = Size.Full();
        Height = Size.Full();
        RemoveParentPadding = true;
    }

    /// <summary> Gets or sets the index of the currently selected tab. </summary>
    [Prop] public int? SelectedIndex { get; set; }

    /// <summary> Gets or sets the visual style variant for the tabbed interface. </summary>
    [Prop] public TabsVariant Variant { get; set; } = TabsVariant.Content;

    /// <summary> Gets or sets whether to remove any padding inherited from parent containers. </summary>
    [Prop] public bool RemoveParentPadding { get; set; }
    /// <summary> Gets or sets the padding around the tabs layout container. </summary>
    [Prop] public Thickness? Padding { get; set; } = new Thickness(4);

    /// <summary> Gets or sets the event handler for tab selection events. </summary>
    [Event] public Func<Event<TabsLayout, int>, ValueTask>? OnSelect { get; set; }

    /// <summary> Gets or sets the event handler for tab close events. </summary>
    [Event] public Func<Event<TabsLayout, int>, ValueTask>? OnClose { get; set; }

    /// <summary> Gets or sets the event handler for tab refresh events. </summary>
    [Event] public Func<Event<TabsLayout, int>, ValueTask>? OnRefresh { get; set; }

    /// <summary> Gets or sets the event handler for tab reordering events. </summary>
    [Event] public Func<Event<TabsLayout, int[]>, ValueTask>? OnReorder { get; set; }

    /// <summary> Compatibility constructor for Action-based event handlers. </summary>
    /// <param name="onSelect">Optional Action-based event handler for tab selection events.</param>
    /// <param name="onClose">Optional Action-based event handler for tab close events.</param>
    /// <param name="onRefresh">Optional Action-based event handler for tab refresh events.</param>
    /// <param name="onReorder">Optional Action-based event handler for tab reordering events.</param>
    /// <param name="selectedIndex">The index of the initially selected tab.</param>
    /// <param name="tabs">Variable number of Tab objects defining the tab structure.</param>
    public TabsLayout(Action<Event<TabsLayout, int>>? onSelect, Action<Event<TabsLayout, int>>? onClose, Action<Event<TabsLayout, int>>? onRefresh, Action<Event<TabsLayout, int[]>>? onReorder, int? selectedIndex, params Tab[] tabs)
        : this(
            onSelect != null ? e => { onSelect(e); return ValueTask.CompletedTask; }
    : null,
            onClose != null ? e => { onClose(e); return ValueTask.CompletedTask; }
    : null,
            onRefresh != null ? e => { onRefresh(e); return ValueTask.CompletedTask; }
    : null,
            onReorder != null ? e => { onReorder(e); return ValueTask.CompletedTask; }
    : null,
            selectedIndex, tabs)
    {
    }
}

/// <summary>
/// Provides extension methods for the TabsLayout widget.
/// </summary>
public static class TabsLayoutExtensions
{
    /// <summary> Sets the visual style variant for the tabbed interface. </summary>
    /// <param name="tabsLayout">The TabsLayout to configure.</param>
    /// <param name="variant">The visual style variant to apply to the tabs.</param>
    public static TabsLayout Variant(this TabsLayout tabsLayout, TabsVariant variant)
    {
        return tabsLayout with { Variant = variant };
    }

    /// <summary> Sets whether to remove any padding inherited from parent containers.</summary>
    /// <param name="tabsLayout">The TabsLayout to configure.</param>
    /// <param name="removeParentPadding">Whether to remove parent padding. Default is true.</param>
    public static TabsLayout RemoveParentPadding(this TabsLayout tabsLayout, bool removeParentPadding = true)
    {
        return tabsLayout with { RemoveParentPadding = removeParentPadding };
    }

    /// <summary> Sets the padding around the tabs layout container using a Thickness object. </summary>
    /// <param name="tabsLayout">The TabsLayout to configure.</param>
    /// <param name="padding">The Thickness object containing left, top, right, and bottom padding values.</param>
    public static TabsLayout Padding(this TabsLayout tabsLayout, Thickness? padding)
    {
        return tabsLayout with { Padding = padding };
    }

    /// <summary> Sets uniform padding around the tabs layout container on all sides. </summary>
    /// <param name="tabsLayout">The TabsLayout to configure.</param>
    /// <param name="padding">The uniform padding value to apply to all sides in units.</param>
    public static TabsLayout Padding(this TabsLayout tabsLayout, int padding)
    {
        return tabsLayout with { Padding = new Thickness(padding) };
    }

    /// <summary> Sets different padding values for vertical and horizontal directions. </summary>
    /// <param name="tabsLayout">The TabsLayout to configure.</param>
    /// <param name="verticalPadding">The padding value for top and bottom sides in units.</param>
    /// <param name="horizontalPadding">The padding value for left and right sides in units.</param>
    public static TabsLayout Padding(this TabsLayout tabsLayout, int verticalPadding, int horizontalPadding)
    {
        return tabsLayout with { Padding = new Thickness(horizontalPadding, verticalPadding) };
    }

    /// <summary> Sets specific padding values for each side of the tabs layout container. </summary>
    /// <param name="tabsLayout">The TabsLayout to configure.</param>
    /// <param name="left">The padding value for the left side in units.</param>
    /// <param name="top">The padding value for the top side in units.</param>
    /// <param name="right">The padding value for the right side in units.</param>
    /// <param name="bottom">The padding value for the bottom side in units.</param>
    public static TabsLayout Padding(this TabsLayout tabsLayout, int left, int top, int right, int bottom)
    {
        return tabsLayout with { Padding = new Thickness(left, top, right, bottom) };
    }
}

/// <summary> Represents an individual tab within a TabsLayout that contains a title, optional icon, optional badge, and content. </summary>
public record Tab : WidgetBase<Tab>
{
    /// <summary> Initializes a new instance of the Tab class with the specified title and optional content. </summary>
    /// <param name="title">The text label displayed in the tab header for user identification and navigation.</param>
    /// <param name="content">Optional content to display in the tab body when the tab is active.</param>
    public Tab(string title, object? content = null) : base(content != null ? [content] : [])
    {
        Title = title;
    }

    /// <summary> Gets or sets the text label displayed in the tab header. </summary>
    [Prop] public string Title { get; set; }

    /// <summary> Gets or sets the optional icon displayed alongside the tab title. </summary>
    [Prop] public Icons? Icon { get; set; }

    /// <summary> Gets or sets the optional badge text displayed on the tab. </summary>
    [Prop] public string? Badge { get; set; }
}

/// <summary> Provides extension methods for the Tab widget. </summary>
public static class TabExtensions
{
    /// <summary> Sets an icon to be displayed alongside the tab title. </summary>
    /// <param name="tab">The Tab to configure.</param>
    /// <param name="icon">The icon to display alongside the tab title, or null to remove the icon.</param>
    public static Tab Icon(this Tab tab, Icons? icon)
    {
        return tab with { Icon = icon };
    }

    /// <summary> Sets a badge to be displayed on the tab. </summary>
    /// <param name="tab">The Tab to configure.</param>
    /// <param name="badge">The badge text to display on the tab, or null to remove the badge.</param>
    public static Tab Badge(this Tab tab, string badge)
    {
        return tab with { Badge = badge };
    }
}
