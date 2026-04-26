
namespace Ivy.Samples.Shared.Apps.Widgets.Wireframe;

[App(icon: Icons.Circle, group: ["Widgets", "Wireframe"], searchHints: ["wireframe", "callout", "annotation", "number", "circle", "marker", "balsamiq"])]
public class WireframeCalloutApp : SampleBase
{
    protected override object? BuildSample()
    {
        var colors = Layout.Horizontal().Gap(4)
            | new WireframeCallout("1")
            | new WireframeCallout("2", Colors.Blue)
            | new WireframeCallout("3", Colors.Green)
            | new WireframeCallout("!", Colors.Pink)
            | new WireframeCallout("?", Colors.Orange)
            | new WireframeCallout("A", Colors.Purple);

        var annotated = Layout.Canvas()
            .Width(Size.Full()).Height(Size.Px(300))
            | new WireframeCallout("1", Colors.Blue)
                .CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(20))
            | new WireframeNote("User signs up", Colors.Yellow)
                .CanvasLeft(Size.Px(60)).CanvasTop(Size.Px(10))
            | new WireframeCallout("2", Colors.Blue)
                .CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(100))
            | new WireframeNote("Verify email", Colors.Blue)
                .CanvasLeft(Size.Px(60)).CanvasTop(Size.Px(90))
            | new WireframeCallout("3", Colors.Green)
                .CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(180))
            | new WireframeNote("Dashboard", Colors.Green)
                .CanvasLeft(Size.Px(60)).CanvasTop(Size.Px(170));

        return Layout.Vertical()
            | Text.H1("Wireframe Callout")
            | Text.H2("Colors")
            | colors
            | Text.H2("Annotated Flow")
            | annotated;
    }
}
