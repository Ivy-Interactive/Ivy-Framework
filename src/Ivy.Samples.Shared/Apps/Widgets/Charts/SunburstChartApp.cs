using Ivy.Charts;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.ChartPie, searchHints: ["visualization", "graph", "analytics", "data", "sunburst", "statistics", "hierarchical"])]
public class SunburstChartApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Grid().Columns(2)
            | new SunburstChart0View()
            | new SunburstChart1View()
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
                    new SunburstNode("Exam", 10),
                    new SunburstNode("Lab", 10),
                    new SunburstNode("Homework", 10)
                }),
                new SunburstNode("Chemistry", 0, new[]
                {
                    new SunburstNode("Experiment", 12),
                    new SunburstNode("Report", 10)
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

        return new Card().Title("School Subjects Breakdown").Height(180)
            | new SunburstChart(data)
                .InnerRadius(100)
                .OuterRadius(260)
                .Padding(4)
                .Tooltip()
                .Legend(new Legend().Horizontal().Center().Bottom())
                .Toolbox()
        ;
    }

}

public class SunburstChart1View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new SunburstNode("Needs", 0, new[]
            {
                new SunburstNode("Housing", 2000),
                new SunburstNode("Transportation", 800),
                new SunburstNode("Utilities", 400),
                new SunburstNode("Groceries", 600),
                new SunburstNode("Health", 300)
            }) { Fill = Colors.Rose },

            new SunburstNode("Wants", 0, new[]
            {
                new SunburstNode("Dining", 500),
                new SunburstNode("Meals", 300),
                new SunburstNode("Shopping", 400),
                new SunburstNode("Travel", 600)
            }) { Fill = Colors.Teal },

            new SunburstNode("Savings", 0, new[]
            {
                new SunburstNode("Investments", 700),
                new SunburstNode("ISK", 300)
            }) { Fill = Colors.Indigo },
        };

        return new Card().Title("Monthly Expense Breakdown").Height(180)
            | new SunburstChart(data)
                .InnerRadius(100)
                .OuterRadius(260)
                .Padding(4)
                .Tooltip()
                .Legend(new Legend().Horizontal().Center().Bottom())
                .Toolbox()
        ;
    }

}
