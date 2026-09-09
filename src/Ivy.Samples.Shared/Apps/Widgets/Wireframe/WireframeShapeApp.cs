namespace Ivy.Samples.Shared.Apps.Widgets.Wireframe;

[App(icon: Icons.Shapes, group: ["Widgets", "Wireframe"], searchHints: ["wireframe", "shape", "polygon", "star", "ellipse", "diamond", "sketch"])]
public class WireframeShapeApp : SampleBase
{
    protected override object? BuildSample()
    {
        var shapes = Layout.Horizontal()
            | new WireframeShape(WireframeShapeKind.Rectangle)
            | new WireframeShape(WireframeShapeKind.Ellipse)
            | new WireframeShape(WireframeShapeKind.Triangle)
            | new WireframeShape(WireframeShapeKind.Diamond)
            | new WireframeShape(WireframeShapeKind.Pentagon);

        var more = Layout.Horizontal()
            | new WireframeShape(WireframeShapeKind.Hexagon)
            | new WireframeShape(WireframeShapeKind.Octagon)
            | new WireframeShape(WireframeShapeKind.Star)
            | new WireframeShape(WireframeShapeKind.Cross)
            | new WireframeShape(WireframeShapeKind.Parallelogram);

        var polygons = Layout.Horizontal()
            | new WireframeShape().Sides(3)
            | new WireframeShape().Sides(7)
            | new WireframeShape().Sides(9)
            | new WireframeShape().Sides(12)
            | new WireframeShape().Sides(20);

        var flow = Layout.Horizontal()
            | new WireframeShape(WireframeShapeKind.Ellipse, "Start", Colors.Green).Filled()
            | new WireframeShape(WireframeShapeKind.Rectangle, "Process", Colors.Sky).Filled()
            | new WireframeShape(WireframeShapeKind.Diamond, "Valid?", Colors.Amber).Filled()
            | new WireframeShape(WireframeShapeKind.Ellipse, "Done", Colors.Green).Filled();

        var densities = Layout.Horizontal()
            | new WireframeShape(WireframeShapeKind.Hexagon, "S").Small()
            | new WireframeShape(WireframeShapeKind.Hexagon, "M")
            | new WireframeShape(WireframeShapeKind.Hexagon, "L").Large();

        return Layout.Vertical()
            | Text.H1("Wireframe Shape")
            | Text.H2("Shapes")
            | shapes
            | more
            | Text.H2("Arbitrary Polygons")
            | polygons
            | Text.H2("Filled With Labels")
            | flow
            | Text.H2("Densities")
            | densities;
    }
}
