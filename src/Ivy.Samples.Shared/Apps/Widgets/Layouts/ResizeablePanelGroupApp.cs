using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.LayoutPanelTop, searchHints: ["split", "resizable", "panels", "divider", "adjustable", "layout"])]
public class ResizeablePanelGroupApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new ResizeablePanelGroup(
            new ResizeablePanel(Size.Fraction(0.25f), "Left"),
            new ResizeablePanel(Size.Fraction(0.75f),
                new ResizeablePanelGroup(
                    new ResizeablePanel(Size.Fraction(0.5f), "Top"),
                    new ResizeablePanel(Size.Fraction(0.5f), "Bottom")
            ).Vertical())
        ).Horizontal().Height(Size.Screen());
    }
}