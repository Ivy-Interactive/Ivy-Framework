using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        OnSelect = onSelect;
        Items = items;
    }

    [Prop] public bool SearchActive { get; set; } = false;

    [Prop] public MenuItem[] Items { get; set; }

    [Event] public Func<Event<SidebarMenu, object>, ValueTask> OnSelect { get; set; }

    [Event] public Func<Event<SidebarMenu, object>, ValueTask>? OnCtrlRightClickSelect { get; set; }

    public static SidebarMenu operator |(SidebarMenu widget, object child)
    {
        throw new NotSupportedException("SidebarMenu does not support children.");
    }

    public SidebarMenu(Action<Event<SidebarMenu, object>> onSelect, params MenuItem[] items)
    : this(e => { onSelect(e); return ValueTask.CompletedTask; }, items)
    {
    }
}

