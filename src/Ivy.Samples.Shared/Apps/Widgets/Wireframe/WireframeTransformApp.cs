namespace Ivy.Samples.Shared.Apps.Widgets.Wireframe;

[App(icon: Icons.RotateCw, group: ["Widgets", "Wireframe"], searchHints: ["wireframe", "transform", "rotate", "scale", "skew", "flip", "sketch"])]
public class WireframeTransformApp : SampleBase
{
    protected override object? BuildSample()
    {
        var rotations = Layout.Horizontal()
            | new WireframeTransform(new WireframeNote("-12 deg")).Rotate(-12)
            | new WireframeTransform(new WireframeNote("0 deg"))
            | new WireframeTransform(new WireframeNote("12 deg")).Rotate(12)
            | new WireframeTransform(new WireframeNote("90 deg")).Rotate(90);

        var scales = Layout.Horizontal()
            | new WireframeTransform(new WireframeShape(WireframeShapeKind.Star, "0.6", Colors.Amber).Filled()).Scale(0.6)
            | new WireframeTransform(new WireframeShape(WireframeShapeKind.Star, "1.0", Colors.Amber).Filled())
            | new WireframeTransform(new WireframeShape(WireframeShapeKind.Star, "1.3", Colors.Amber).Filled()).Scale(1.3);

        var effects = Layout.Horizontal()
            | new WireframeTransform(new WireframeShape(WireframeShapeKind.Rectangle, "Skew", Colors.Sky).Filled()).Skew(-18)
            | new WireframeTransform(new WireframeArrow().Width(Size.Px(120)).Height(Size.Px(60))).FlipHorizontal()
            | new WireframeTransform(new WireframeShape(WireframeShapeKind.Rectangle, "Faded", Colors.Green).Filled()).Opacity(0.35);

        var origins = Layout.Horizontal()
            | new WireframeTransform(new WireframeShape(WireframeShapeKind.Rectangle, "Center")).Rotate(45)
            | new WireframeTransform(new WireframeShape(WireframeShapeKind.Rectangle, "TopLeft")).Rotate(45).Origin(TransformOrigin.TopLeft)
            | new WireframeTransform(new WireframeShape(WireframeShapeKind.Rectangle, "BottomRight")).Rotate(45).Origin(TransformOrigin.BottomRight);

        var scaled = new WireframeTransform(
                new WireframeMockup(
                        Layout.Vertical().Padding(3)
                        | Text.H4("Inbox")
                        | new Button("Compose").Width(Size.Full()))
                    .Width(Size.Px(300))
                    .Height(Size.Px(500)))
            .Scale(0.45)
            .Origin(TransformOrigin.TopLeft);

        return Layout.Vertical()
            | Text.H1("Wireframe Transform")
            | Text.H2("Rotate")
            | rotations
            | Text.H2("Scale")
            | scales
            | Text.H2("Skew, Flip, Opacity")
            | effects
            | Text.H2("Origin")
            | origins
            | Text.H2("Scaling a Whole Mockup")
            | scaled;
    }
}
