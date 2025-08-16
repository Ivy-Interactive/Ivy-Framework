using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.PanelLeft, path: ["Widgets", "Layouts"])]
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