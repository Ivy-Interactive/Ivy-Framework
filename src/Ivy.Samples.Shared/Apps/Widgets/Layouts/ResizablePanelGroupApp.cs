using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.LayoutPanelTop, searchHints: ["split", "resizable", "panels", "divider", "adjustable", "layout"])]
public class ResizablePanelGroupApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new ResizablePanelGroup(
            new ResizablePanel(Size.Fraction(0.25f).Min(0.1f).Max(0.4f), "Left (min: 10%, max: 40%)"),
            new ResizablePanel(Size.Fraction(0.75f).Min(0.6f).Max(0.9f),
                new ResizablePanelGroup(
                    new ResizablePanel(Size.Fraction(0.5f).Min(0.3f), "Top (min: 30%)"),
                    new ResizablePanel(Size.Fraction(0.5f).Max(0.7f), "Bottom (max: 70%)")
            ).Vertical())
        ).Horizontal().Height(Size.Screen());
    }
}