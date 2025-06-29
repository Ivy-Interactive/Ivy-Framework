using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Layouts;

[App(icon: Icons.LayoutPanelTop)]
public class ResizeablePanelGroupApp : ViewBase
{
    public override object? Build()
    {
        return new ResizeablePanelGroup(
            new ResizeablePanel(25, "Left"),
            new ResizeablePanel(75,
                new ResizeablePanelGroup(
                    new ResizeablePanel(50, "Top"),
                    new ResizeablePanel(50, "Bottom")
            ).Vertical())
        ).Horizontal().Height(Size.Screen());
    }
}