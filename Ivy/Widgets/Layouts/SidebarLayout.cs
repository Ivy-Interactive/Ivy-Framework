using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record SidebarLayout : WidgetBase<SidebarLayout>
{
    public SidebarLayout(object mainContent, object sidebarContent, object? sidebarHeader = null, object? sidebarFooter = null)
        : base([new Slot("MainContent", mainContent), new Slot("SidebarContent", sidebarContent), new Slot("SidebarHeader", sidebarHeader), new Slot("SidebarFooter", sidebarFooter)])
    {
    }

    public static SidebarLayout operator |(SidebarLayout widget, object child)
    {
        throw new NotSupportedException("SidebarLayout does not support children.");
    }
}

public record SidebarMenu : WidgetBase<SidebarLayout>
{
    public SidebarMenu(Action<Event<SidebarMenu, object>> onSelect, params MenuItem[] items)
    {
        OnSelect = onSelect;
        Items = items;
    }

    [Prop] public bool SearchActive { get; set; } = false;
    [Prop] public MenuItem[] Items { get; set; }
    [Event] public Action<Event<SidebarMenu, object>> OnSelect { get; set; }
    [Event] public Action<Event<SidebarMenu, object>>? OnCtrlRightClickSelect { get; set; }


    public static SidebarMenu operator |(SidebarMenu widget, object child)
    {
        throw new NotSupportedException("SidebarMenu does not support children.");
    }
}

