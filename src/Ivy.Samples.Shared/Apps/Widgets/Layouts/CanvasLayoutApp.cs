
namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.Move, group: ["Widgets", "Layouts"], searchHints: ["canvas", "absolute", "position", "freeform", "wireframe", "coordinates"])]
public class CanvasLayoutApp : SampleBase
{
    protected override object? BuildSample()
    {
        var basic = Layout.Canvas()
            .Width(Size.Full()).Height(Size.Px(200))
            | new Badge("Top Left").CanvasLeft(Size.Px(10)).CanvasTop(Size.Px(10))
            | new Badge("Center").Info().CanvasLeft(Size.Px(200)).CanvasTop(Size.Px(80))
            | new Badge("Bottom Right").Success().CanvasLeft(Size.Px(350)).CanvasTop(Size.Px(150));

        var wireframe = Layout.Canvas()
            .Width(Size.Full()).Height(Size.Px(350))
            | new WireframeNote("User clicks Login", Colors.Yellow)
                .CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(10))
            | new WireframeNote("Validate credentials\nagainst OAuth", Colors.Blue)
                .CanvasLeft(Size.Px(200)).CanvasTop(Size.Px(70))
            | new WireframeNote("Token exchange\n+ session create", Colors.Green)
                .CanvasLeft(Size.Px(380)).CanvasTop(Size.Px(20))
            | new WireframeNote("Redirect to\ndashboard", Colors.Purple)
                .CanvasLeft(Size.Px(380)).CanvasTop(Size.Px(170))
            | new WireframeNote("Show error toast", Colors.Pink)
                .CanvasLeft(Size.Px(200)).CanvasTop(Size.Px(230))
            | new WireframeNote("Retry?", Colors.Orange)
                .CanvasLeft(Size.Px(20)).CanvasTop(Size.Px(250));

        return Layout.Vertical()
            | Text.H1("Canvas Layout")
            | Text.H2("Basic Positioning")
            | basic
            | Text.H2("Wireframe Sketch")
            | wireframe;
    }
}
