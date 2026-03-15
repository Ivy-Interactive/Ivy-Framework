namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.RectangleHorizontal, path: ["Widgets"], searchHints: ["aspect", "ratio", "proportion", "size", "responsive"])]
public class AspectRatioApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Aspect Ratio")
               | Text.P("Use the AspectRatio extension method to maintain proportional dimensions on any widget.")
               | Text.H2("Common Ratios")
               | Layout.Horizontal(
                   new Box(Text.Block("16:9")).Width(Size.Units(80)).AspectRatio(16f / 9f).Background(Colors.Primary),
                   new Box(Text.Block("4:3")).Width(Size.Units(80)).AspectRatio(4f / 3f).Background(Colors.Secondary),
                   new Box(Text.Block("1:1")).Width(Size.Units(40)).AspectRatio(1f).Background(Colors.Warning)
               ).Gap(4);
    }
}
