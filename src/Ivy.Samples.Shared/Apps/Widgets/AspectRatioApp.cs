namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.RectangleHorizontal, path: ["Widgets"], searchHints: ["aspect", "ratio", "proportion", "size", "responsive"])]
public class AspectRatioApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical().Gap(8)
               | Text.H1("Aspect Ratio")
               | Text.P("Use the AspectRatio extension method to maintain proportional dimensions on any widget. Set a width and the height is computed automatically from the ratio.")
               | Text.H2("Common Ratios")
               | (Layout.Horizontal().Gap(4)
                   | new Box(Text.Block("16:9 Widescreen")).Width(Size.Units(80)).AspectRatio(16f / 9f).Background(Colors.Primary)
                   | new Box(Text.Block("4:3 Classic")).Width(Size.Units(80)).AspectRatio(4f / 3f).Background(Colors.Secondary)
                   | new Box(Text.Block("1:1 Square")).Width(Size.Units(40)).AspectRatio(1f).Background(Colors.Warning))
               | Text.H2("On Different Widgets")
               | (Layout.Horizontal().Gap(4)
                   | new Card(Text.Block("Card with 16:9 ratio")).Width(Size.Units(80)).AspectRatio(16f / 9f)
                   | new Card(Text.Block("Card with 1:1 ratio")).Width(Size.Units(40)).AspectRatio(1f))
               | Text.H2("Scaling with Width")
               | Text.P("Same 1:1 ratio at different widths — the box scales proportionally.")
               | (Layout.Horizontal().Gap(4)
                   | new Box(Text.Block("Small")).Width(Size.Units(20)).AspectRatio(1f).Background(Colors.Red)
                   | new Box(Text.Block("Medium")).Width(Size.Units(40)).AspectRatio(1f).Background(Colors.Orange)
                   | new Box(Text.Block("Large")).Width(Size.Units(60)).AspectRatio(1f).Background(Colors.Amber));
    }
}
