using Ivy.Charts;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.ChartPie, searchHints: ["visualization", "graph", "analytics", "data", "sunburst", "statistics", "hierarchical"])]
public class SunburstChartApp : SampleBase
{
  protected override object? BuildSample()
  {
    return Layout.Grid().Columns(1)
        | new SunburstChart0View()
    ;
  }
}

public class SunburstChart0View : ViewBase
{
  public override object? Build()
  {
    var data = new[]
    {
            new SunburstNode("German Writing", 0, new[]
            {
                new SunburstNode("Presentation", 0, new[]
                {
                    new SunburstNode("\"Analysis of\nFragmented\nExpressions in\nGerman\"", 10)
                }),
                new SunburstNode("Essay", 10)
            }) { Fill = Colors.Sky },

            new SunburstNode("German Literature", 0, new[]
            {
                new SunburstNode("Schiller", 0, new[]
                {
                    new SunburstNode("\"Kabale und\nLiebe\"", 10)
                }),
                new SunburstNode("Kafka", 10)
            }) { Fill = Colors.Orange },

            new SunburstNode("German History\nof Art", 0, new[]
            {
                new SunburstNode("Art Styles", 0, new[]
                {
                    new SunburstNode("realism", 10)
                }),
                new SunburstNode("Work", 10),
                new SunburstNode("Artist", 10)
            }) { Fill = Colors.Amber },

            new SunburstNode("Spoken German", 0, new[]
            {
                new SunburstNode("Chat", 10)
            }) { Fill = Colors.Zinc },
        };

    return new Card().Title("Language Studies")
        | new SunburstChart(data)
            .InnerRadius("30%")
            .OuterRadius("90%")
            .Padding(4)
            .StartAngle(180)
            .Tooltip()
    ;
  }
}
