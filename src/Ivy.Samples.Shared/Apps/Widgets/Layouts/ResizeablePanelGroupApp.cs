using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.LayoutPanelTop, searchHints: ["split", "resizable", "panels", "divider", "adjustable", "layout"])]
public class ResizeablePanelGroupApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new ResizeablePanelGroup(
            new ResizeablePanel(Size.Fraction(0.25f).Min(0.1f).Max(0.4f), "Left (min: 10%, max: 40%)"),
            new ResizeablePanel(Size.Fraction(0.75f).Min(0.6f).Max(0.9f),
                new ResizeablePanelGroup(
                    new ResizeablePanel(Size.Fraction(0.5f).Min(0.3f), "Top (min: 30%)"),
                    new ResizeablePanel(Size.Fraction(0.5f).Max(0.7f), "Bottom (max: 70%)")
            ).Vertical())
        ).Horizontal().Height(Size.Screen());
    }
}