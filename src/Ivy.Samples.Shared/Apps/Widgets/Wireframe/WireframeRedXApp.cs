namespace Ivy.Samples.Shared.Apps.Widgets.Wireframe;

[App(icon: Icons.X, group: ["Widgets", "Wireframe"], searchHints: ["wireframe", "red x", "cross", "reject", "wrong", "remove", "sketch"])]
public class WireframeRedXApp : SampleBase
{
    protected override object? BuildSample()
    {
        var sizes = Layout.Horizontal()
            | new WireframeRedX()
            | new WireframeRedX().Width(Size.Px(120)).Height(Size.Px(120))
            | new WireframeRedX().Width(Size.Px(80)).Height(Size.Px(40));

        var colors = Layout.Horizontal()
            | new WireframeRedX().Width(Size.Px(140)).Height(Size.Px(70))
            | new WireframeRedX(Colors.Slate).Width(Size.Px(140)).Height(Size.Px(70))
            | new WireframeRedX(Colors.Violet).Width(Size.Px(140)).Height(Size.Px(70))
            | new WireframeRedX(Colors.Green).Width(Size.Px(140)).Height(Size.Px(70));

        var densities = Layout.Horizontal()
            | new WireframeRedX().Small().Width(Size.Px(140)).Height(Size.Px(70))
            | new WireframeRedX().Width(Size.Px(140)).Height(Size.Px(70))
            | new WireframeRedX().Large().Width(Size.Px(140)).Height(Size.Px(70));

        return Layout.Vertical()
            | Text.H1("Wireframe Red X")
            | Text.H2("Sizes")
            | sizes
            | Text.H2("Colors")
            | colors
            | Text.H2("Densities")
            | densities;
    }
}
