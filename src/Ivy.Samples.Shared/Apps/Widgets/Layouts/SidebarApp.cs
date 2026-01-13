using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.PanelLeft, path: ["Widgets", "Layouts"], searchHints: ["navigation", "menu", "drawer", "side-panel", "layout", "aside"])]
public class SidebarApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new SidebarLayout(
            "MainContent",
            "SidebarContent",
            "SidebarHeader",
            "SidebarFooter"
        );
    }
}