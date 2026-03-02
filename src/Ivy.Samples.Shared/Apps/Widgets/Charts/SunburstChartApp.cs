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
            new SunburstNode("Science", 0, new[]
            {
                new SunburstNode("Biology", 0, new[]
                {
                    new SunburstNode("Exam", 15),
                    new SunburstNode("Lab", 10),
                    new SunburstNode("Homework", 5)
                }),
                new SunburstNode("Chemistry", 0, new[]
                {
                    new SunburstNode("Experiment", 12),
                    new SunburstNode("Report", 8)
                }),
                new SunburstNode("Physics", 0, new[]
                {
                    new SunburstNode("Quiz", 10)
                })
            }) { Fill = Colors.Sky },

            new SunburstNode("Mathematics", 0, new[]
            {
                new SunburstNode("Algebra", 0, new[]
                {
                    new SunburstNode("Midterm", 20),
                    new SunburstNode("Equations", 10)
                }),
                new SunburstNode("Calculus", 0, new[]
                {
                    new SunburstNode("Derivatives", 15),
                    new SunburstNode("Integrals", 15)
                })
            }) { Fill = Colors.Orange },

            new SunburstNode("History", 0, new[]
            {
                new SunburstNode("World", 0, new[]
                {
                    new SunburstNode("Essay", 25)
                }),
                new SunburstNode("European", 0, new[]
                {
                    new SunburstNode("Presentation", 15)
                })
            }) { Fill = Colors.Amber },

            new SunburstNode("English", 0, new[]
            {
                new SunburstNode("Literature", 0, new[]
                {
                    new SunburstNode("Reading", 10),
                    new SunburstNode("Analysis", 20)
                }),
                new SunburstNode("Grammar", 0, new[]
                {
                    new SunburstNode("Test", 10)
                })
            }) { Fill = Colors.Zinc },
        };

        return new Card().Title("Subject Breakdown").Height(160)
            | new SunburstChart(data)
                .InnerRadius(100)
                .OuterRadius(260)
                .Padding(4)
                .Tooltip()
        ;
    }

}
