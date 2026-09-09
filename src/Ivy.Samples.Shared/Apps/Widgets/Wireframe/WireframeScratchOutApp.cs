namespace Ivy.Samples.Shared.Apps.Widgets.Wireframe;

[App(icon: Icons.Highlighter, group: ["Widgets", "Wireframe"], searchHints: ["wireframe", "scratch", "scribble", "strike", "cross out", "remove", "sketch"])]
public class WireframeScratchOutApp : SampleBase
{
    protected override object? BuildSample()
    {
        var sizes = Layout.Horizontal()
            | new WireframeScratchOut()
            | new WireframeScratchOut().Width(Size.Px(120)).Height(Size.Px(120))
            | new WireframeScratchOut().Width(Size.Px(80)).Height(Size.Px(40));

        var colors = Layout.Horizontal()
            | new WireframeScratchOut()
            | new WireframeScratchOut(Colors.Red).Width(Size.Px(140)).Height(Size.Px(70))
            | new WireframeScratchOut(Colors.Sky).Width(Size.Px(140)).Height(Size.Px(70))
            | new WireframeScratchOut(Colors.Green).Width(Size.Px(140)).Height(Size.Px(70));

        var densities = Layout.Horizontal()
            | new WireframeScratchOut().Small().Width(Size.Px(140)).Height(Size.Px(70))
            | new WireframeScratchOut().Width(Size.Px(140)).Height(Size.Px(70))
            | new WireframeScratchOut().Large().Width(Size.Px(140)).Height(Size.Px(70));

        return Layout.Vertical()
            | Text.H1("Wireframe Scratch Out")
            | Text.H2("Sizes")
            | sizes
            | Text.H2("Colors")
            | colors
            | Text.H2("Densities")
            | densities;
    }
}
