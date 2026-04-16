using System.Runtime.CompilerServices;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A common application layout with a collapsible sidebar and main content area.
/// </summary>
public record SidebarLayout : WidgetBase<SidebarLayout>
{
    public static Size DefaultWidth => Size.Rem(16);

    public SidebarLayout(object mainContent, object sidebarContent, object? sidebarHeader = null, object? sidebarFooter = null, Size? width = null)
    : base(BuildSlots(mainContent, sidebarContent, sidebarHeader, sidebarFooter))
    {
        Width = (width ?? DefaultWidth).ToResponsive();
    }

    private static Slot[] BuildSlots(object mainContent, object sidebarContent, object? sidebarHeader, object? sidebarFooter) =>
    [
        new Slot("MainContent", mainContent),
        new Slot("SidebarContent", sidebarContent),
        sidebarHeader != null ? new Slot("SidebarHeader", sidebarHeader) : new Slot("SidebarHeader"),
        sidebarFooter != null ? new Slot("SidebarFooter", sidebarFooter) : new Slot("SidebarFooter")
    ];

    internal SidebarLayout() { }

    [Prop] public bool MainAppSidebar { get; set; } = false;

    [Prop] public bool Open { get; set; } = true;

    [Prop] public int MainContentPadding { get; set; } = 2;

    [Prop] public bool Resizable { get; set; } = false;

    public static SidebarLayout operator |(SidebarLayout widget, object child)
    {
        throw new NotSupportedException("SidebarLayout does not support children.");
    }
}

public static class SidebarLayoutExtensions
{
    public static SidebarLayout MainAppSidebar(this SidebarLayout sidebar, bool isMainApp = true)
    {
        return sidebar with { MainAppSidebar = isMainApp };
    }

    public static SidebarLayout Open(this SidebarLayout sidebar, bool open = true)
    {
        return sidebar with { Open = open };
    }

    public static SidebarLayout Padding(this SidebarLayout sidebar, int padding)
    {
        return sidebar with { MainContentPadding = padding };
    }

    /// <summary>
    /// Enables drag-to-resize on the sidebar border. Users can drag to adjust the sidebar width at runtime.
    /// Use .Width(Size.Px(300).Min(Size.Px(200)).Max(Size.Px(600))) to customize constraints.
    /// Default constraints when resizable is 200px min and 600px max.
    /// </summary>
    public static SidebarLayout Resizable(this SidebarLayout sidebar, bool resizable = true)
    {
        if (!resizable)
        {
            return sidebar with { Resizable = false };
        }

        // Apply default min/max constraints if not already set on the Width
        var width = sidebar.Width?.Default ?? SidebarLayout.DefaultWidth;
        if (width.Min == null)
        {
            width = width.Min(Size.Px(200));
        }
        if (width.Max == null)
        {
            width = width.Max(Size.Px(600));
        }

        return (sidebar with { Resizable = true }).Width(width);
    }
}

public record SidebarMenu : WidgetBase<SidebarLayout>
{
    [OverloadResolutionPriority(1)]
    public SidebarMenu(Func<Event<SidebarMenu, object>, ValueTask> onSelect, params MenuItem[] items)
    {
        OnSelect = new(onSelect);
        Items = items;
    }

    [Prop] public bool SearchActive { get; set; } = false;

    [Prop] public MenuItem[] Items { get; set; }

    [Event] public EventHandler<Event<SidebarMenu, object>> OnSelect { get; set; }

    [Event] public EventHandler<Event<SidebarMenu, object>>? OnCtrlRightClickSelect { get; set; }

    public static SidebarMenu operator |(SidebarMenu widget, object child)
    {
        throw new NotSupportedException("SidebarMenu does not support children.");
    }

    public SidebarMenu(Action<Event<SidebarMenu, object>> onSelect, params MenuItem[] items)
    : this(onSelect.ToValueTask(), items)
    {
    }
}

