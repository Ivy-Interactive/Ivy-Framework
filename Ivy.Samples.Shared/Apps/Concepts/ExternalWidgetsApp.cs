using ExternalWidgetExample;
using Ivy.Shared;
using Ivy.Views;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Puzzle, searchHints: ["external", "plugin", "nuget", "package", "custom", "widget"])]
public class ExternalWidgetsApp : SampleBase
{
    protected override object? BuildSample()
    {
        var clickedIndex = UseState<int?>(-1);

        return Layout.Vertical(
            new Card(header: "External Widget Demo") | Layout.Vertical(
                Text.P("This SuperChart widget is loaded from an external NuGet package (ExternalWidgetExample)."),
                Text.P("The React component is bundled as an embedded resource and loaded dynamically.").Muted()
            ).Gap(0),

            new SuperChart("Monthly Sales", [12, 19, 8, 25, 32, 18, 42, 35, 28, 45, 38, 52])
                .Color("#3b82f6")
                .ShowLabels()
                .HandlePointClick(index => clickedIndex.Value = index),

            clickedIndex.Value >= 0
                ? Callout.Info($"You clicked bar at index {clickedIndex.Value}")
                : null
        );
    }
}
