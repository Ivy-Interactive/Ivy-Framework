namespace Ivy.Samples.Shared.Apps.Widgets.Wireframe;

[App(icon: Icons.Image, group: ["Widgets", "Wireframe"], searchHints: ["wireframe", "placeholder", "image", "box", "crossed", "sketch"])]
public class WireframePlaceholderApp : SampleBase
{
    protected override object? BuildSample()
    {
        var sizes = Layout.Horizontal()
            | new WireframePlaceholder("Image Placeholder")
            | new WireframePlaceholder("Square").Width(Size.Px(140)).Height(Size.Px(140))
            | new WireframePlaceholder().Width(Size.Px(100)).Height(Size.Px(70));

        var colors = Layout.Horizontal()
            | new WireframePlaceholder("Violet")
            | new WireframePlaceholder("Sky", Colors.Sky)
            | new WireframePlaceholder("Green", Colors.Green)
            | new WireframePlaceholder("Gray", Colors.Gray);

        var densities = Layout.Horizontal()
            | new WireframePlaceholder("Small").Small()
            | new WireframePlaceholder("Medium")
            | new WireframePlaceholder("Large").Large();

        var inLayout = Layout.Vertical()
            | new WireframePlaceholder("Hero image").Width(Size.Full()).Height(Size.Px(180)).Color(Colors.Sky)
            | (Layout.Horizontal()
                | new WireframePlaceholder("Chart").Width(Size.Px(220)).Height(Size.Px(140))
                | new WireframePlaceholder("Map").Width(Size.Px(220)).Height(Size.Px(140)));

        return Layout.Vertical()
            | Text.H1("Wireframe Placeholder")
            | Text.H2("Sizes")
            | sizes
            | Text.H2("Colors")
            | colors
            | Text.H2("Densities")
            | densities
            | Text.H2("In a Layout")
            | inLayout;
    }
}
