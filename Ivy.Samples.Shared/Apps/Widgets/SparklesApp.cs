using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Star, path: ["Widgets"], searchHints: ["sparkles", "badge", "decoration"])]
public class SparklesApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical().Gap(4)
            | new Sparkles()
            | new Sparkles().Text("Shiny!")
            | new Sparkles().Color(Colors.Yellow).Size(Sizes.Large);
    }
}


