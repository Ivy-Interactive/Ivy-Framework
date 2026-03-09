
namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.PanelLeft, path: ["Widgets", "Layouts"], searchHints: ["navigation", "menu", "drawer", "side-panel", "layout", "aside", "resizable", "drag-to-resize"])]
public class SidebarApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new SidebarLayout(
            "MainContent",
            "SidebarContent",
            "SidebarHeader",
            "SidebarFooter"
        ).Resizable();
    }
}
