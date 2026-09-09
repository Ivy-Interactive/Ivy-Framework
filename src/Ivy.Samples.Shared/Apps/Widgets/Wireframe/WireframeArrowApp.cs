namespace Ivy.Samples.Shared.Apps.Widgets.Wireframe;

[App(icon: Icons.ArrowRight, group: ["Widgets", "Wireframe"], searchHints: ["wireframe", "arrow", "pointer", "connector", "flow", "sketch"])]
public class WireframeArrowApp : SampleBase
{
    protected override object? BuildSample()
    {
        var directions = Layout.Horizontal()
            | new WireframeArrow()
            | new WireframeArrow(ArrowDirection.Left)
            | new WireframeArrow(ArrowDirection.Up).Width(Size.Px(60)).Height(Size.Px(120))
            | new WireframeArrow(ArrowDirection.Down).Width(Size.Px(60)).Height(Size.Px(120))
            | new WireframeArrow(ArrowDirection.DownRight).Width(Size.Px(140)).Height(Size.Px(100));

        var heads = Layout.Horizontal()
            | new WireframeArrow().Heads(ArrowHeads.End)
            | new WireframeArrow().Heads(ArrowHeads.Start)
            | new WireframeArrow().Heads(ArrowHeads.Both)
            | new WireframeArrow().Heads(ArrowHeads.None);

        var bends = Layout.Horizontal()
            | new WireframeArrow()
            | new WireframeArrow().Bend(ArrowBend.Left)
            | new WireframeArrow().Bend(ArrowBend.Right);

        var dashed = Layout.Horizontal()
            | new WireframeArrow().Dashed()
            | new WireframeArrow(ArrowDirection.Right, Colors.Red).Dashed().Bend(ArrowBend.Left)
            | new WireframeArrow(ArrowDirection.Right, Colors.Sky).Dashed().Heads(ArrowHeads.Both);

        var densities = Layout.Horizontal()
            | new WireframeArrow().Small()
            | new WireframeArrow()
            | new WireframeArrow().Large();

        return Layout.Vertical()
            | Text.H1("Wireframe Arrow")
            | Text.H2("Directions")
            | directions
            | Text.H2("Heads")
            | heads
            | Text.H2("Bends")
            | bends
            | Text.H2("Dashed")
            | dashed
            | Text.H2("Densities")
            | densities;
    }
}
