namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(
    icon: Icons.Milestone,
    isVisible: false,
    searchHints: ["reference line", "release", "marker", "vertical line", "annotation", "chart", "nuget"])]
public class ReferenceLinesChartApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Grid().Columns(2)
            | new ReferenceLinesLineChartView()
            | new ReferenceLinesBarChartView()
            | new ReferenceLinesAreaChartView()
            | new ReferenceLinesHorizontalThresholdView();
    }
}

/// <summary>
/// Vertical dashed lines at category indices (typical for “release shipped on this bucket”).
/// Indices are 0-based and match the order of the dimension values after the pivot runs.
/// </summary>
public class ReferenceLinesLineChartView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Week = "W1", Downloads = 900 },
            new { Week = "W2", Downloads = 1100 },
            new { Week = "W3", Downloads = 1300 },
            new { Week = "W4", Downloads = 1250 },
            new { Week = "W5", Downloads = 1400 },
            new { Week = "W6", Downloads = 1800 },
            new { Week = "W7", Downloads = 1750 },
            new { Week = "W8", Downloads = 1900 },
            new { Week = "W9", Downloads = 2100 },
            new { Week = "W10", Downloads = 2400 },
            new { Week = "W11", Downloads = 2300 },
            new { Week = "W12", Downloads = 2500 },
        };

        return new Card().Title("Line chart — Ivy releases (vertical reference lines)")
            | data.ToLineChart(
                    style: LineChartStyles.Dashboard,
                    polish: c => c
                        .ReferenceLine(new ReferenceLine(2, null, "Ivy 0.9").StrokeWidth(2))
                        .ReferenceLine(new ReferenceLine(6, null, "Ivy 1.0").StrokeWidth(2))
                        .ReferenceLine(new ReferenceLine(10, null, "Ivy 1.5").StrokeWidth(2))
                        .Toolbox())
                .Dimension("Week", e => e.Week)
                .Measure("Downloads", e => e.Sum(f => f.Downloads));
    }
}

public class ReferenceLinesBarChartView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Items = 40 },
            new { Month = "Feb", Items = 52 },
            new { Month = "Mar", Items = 48 },
            new { Month = "Apr", Items = 61 },
            new { Month = "May", Items = 55 },
            new { Month = "Jun", Items = 70 },
        };

        return new Card().Title("Bar chart — same markers on category axis")
            | data.ToBarChart(
                    style: BarChartStyles.Default,
                    polish: b => b
                        .ReferenceLine(1, null, "Milestone A")
                        .ReferenceLine(4, null, "Milestone B")
                        .Toolbox())
                .Dimension("Month", e => e.Month)
                .Measure("Items", e => e.Sum(f => f.Items));
    }
}

public class ReferenceLinesAreaChartView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Day = "Mon", Total = 20 },
            new { Day = "Tue", Total = 35 },
            new { Day = "Wed", Total = 28 },
            new { Day = "Thu", Total = 42 },
            new { Day = "Fri", Total = 38 },
        };

        return new Card().Title("Area chart — deploy markers")
            | data.ToAreaChart(
                    style: AreaChartStyles.Default,
                    polish: a => a.ReferenceLine(2, null, "Deploy").Toolbox())
                .Dimension("Day", e => e.Day)
                .Measure("Total", e => e.Sum(f => f.Total));
    }
}

/// <summary>
/// Horizontal reference line: set x to null and y to the value on the value axis.
/// </summary>
public class ReferenceLinesHorizontalThresholdView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Sprint = "S1", Points = 12 },
            new { Sprint = "S2", Points = 18 },
            new { Sprint = "S3", Points = 15 },
            new { Sprint = "S4", Points = 22 },
            new { Sprint = "S5", Points = 19 },
        };

        const int goal = 16;

        return new Card().Title("Line chart — horizontal threshold (y-axis reference)")
            | data.ToLineChart(
                    style: LineChartStyles.Default,
                    polish: c => c
                        .ReferenceLine(null, goal, $"Goal: {goal} pts")
                        .Toolbox())
                .Dimension("Sprint", e => e.Sprint)
                .Measure("Points", e => e.Sum(f => f.Points));
    }
}
