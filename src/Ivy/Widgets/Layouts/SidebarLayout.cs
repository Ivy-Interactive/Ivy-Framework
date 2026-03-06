using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A common application layout with a collapsible sidebar and main content area.
/// </summary>
public record SidebarLayout : WidgetBase<SidebarLayout>
{
    public static Size DefaultWidth => Size.Rem(16);

    public SidebarLayout(object mainContent, object sidebarContent, object? sidebarHeader = null, object? sidebarFooter = null, Size? width = null)
    : base([new Slot("MainContent", mainContent), new Slot("SidebarContent", sidebarContent), new Slot("SidebarHeader", sidebarHeader), new Slot("SidebarFooter", sidebarFooter)])
    {
        Width = width ?? DefaultWidth;
    }

    internal SidebarLayout() { }

    [Prop] public bool MainAppSidebar { get; set; } = false;

    [Prop] public int MainContentPadding { get; set; } = 2;

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

    public static SidebarLayout Padding(this SidebarLayout sidebar, int padding)
    {
        return sidebar with { MainContentPadding = padding };
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

