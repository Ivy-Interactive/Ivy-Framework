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
                    new SunburstNode("Exam", 8.5),
                    new SunburstNode("Lab", 8.5),
                    new SunburstNode("Homework", 8.5)
                }),
                new SunburstNode("Chemistry", 0, new[]
                {
                    new SunburstNode("Experiment", 10.2),
                    new SunburstNode("Report", 8.5)
                }),
                new SunburstNode("Physics", 0, new[]
                {
                    new SunburstNode("Quiz", 8.5)
                })
            }) { Fill = Colors.Sky },

            new SunburstNode("Mathematics", 0, new[]
            {
                new SunburstNode("Algebra", 0, new[]
                {
                    new SunburstNode("Midterm", 17),
                    new SunburstNode("Equations", 8.5)
                }),
                new SunburstNode("Calculus", 0, new[]
                {
                    new SunburstNode("Derivatives", 12.75),
                    new SunburstNode("Integrals", 12.75)
                })
            }) { Fill = Colors.Orange },

            new SunburstNode("History", 0, new[]
            {
                new SunburstNode("World", 0, new[]
                {
                    new SunburstNode("Essay", 21.25)
                }),
                new SunburstNode("European", 0, new[]
                {
                    new SunburstNode("Presentation", 12.75)
                })
            }) { Fill = Colors.Amber },

            new SunburstNode("English", 0, new[]
            {
                new SunburstNode("Literature", 0, new[]
                {
                    new SunburstNode("Reading", 8.5),
                    new SunburstNode("Analysis", 17)
                }),
                new SunburstNode("Grammar", 0, new[]
                {
                    new SunburstNode("Test", 8.5)
                })
            }) { Fill = Colors.Zinc },
        };

        return new Card().Title("School Subjects Breakdown").Height(153)
            | new SunburstChart(data)
                .InnerRadius(85)
                .OuterRadius(221)
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
                new SunburstNode("Housing", 1700),
                new SunburstNode("Transportation", 680),
                new SunburstNode("Utilities", 340),
                new SunburstNode("Groceries", 510),
                new SunburstNode("Health", 255)
            }) { Fill = Colors.Rose },

            new SunburstNode("Wants", 0, new[]
            {
                new SunburstNode("Dining", 425),
                new SunburstNode("Meals", 255),
                new SunburstNode("Shopping", 340),
                new SunburstNode("Travel", 510)
            }) { Fill = Colors.Teal },

            new SunburstNode("Savings", 0, new[]
            {
                new SunburstNode("Investments", 595),
                new SunburstNode("ISK", 255)
            }) { Fill = Colors.Indigo },
        };

        return new Card().Title("Monthly Expense Breakdown").Height(153)
            | new SunburstChart(data)
                .InnerRadius(85)
                .OuterRadius(221)
                .Padding(4)
                .Tooltip()
                .Legend(new Legend().Horizontal().Center().Bottom())
                .Toolbox()
        ;
    }

}
