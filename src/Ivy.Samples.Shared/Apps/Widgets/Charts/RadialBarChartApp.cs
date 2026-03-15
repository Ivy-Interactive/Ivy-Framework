
namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.ChartPie, searchHints: ["visualization", "graph", "analytics", "data", "radial", "circular", "progress", "gauge", "statistics"])]
public class RadialBarChartApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Grid().Columns(3)
            | new RadialBarChart0()
            | new RadialBarChart1()
            | new RadialBarChart2()
            | new RadialBarChart3()
            | new RadialBarChart4()
            | new RadialBarChart5()
        ;
    }
}

public class RadialBarChart0 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Category = "Completed", Value = 85 },
            new { Category = "In Progress", Value = 65 },
            new { Category = "Pending", Value = 45 },
            new { Category = "Blocked", Value = 25 }
        };

        return new Card().Title("Basic Radial Bar Chart")
            | new RadialBarChart(data)
                .ColorScheme(ColorScheme.Default)
                .RadialBar("Value")
                .Tooltip()
                .Legend()
        ;
    }
}

public class RadialBarChart1 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Goal = "Move", Progress = 78 },
            new { Goal = "Exercise", Progress = 62 },
            new { Goal = "Stand", Progress = 90 }
        };

        return new Card().Title("Fitness Goals (Apple Watch Style)")
            | new RadialBarChart(data)
                .ColorScheme(ColorScheme.Default)
                .RadialBar(new RadialBar("Progress").Background(true))
                .InnerRadius("20%")
                .OuterRadius("90%")
                .StartAngle(90)
                .EndAngle(450)
                .Tooltip()
        ;
    }
}

public class RadialBarChart2 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Metric = "Performance", Score = 88 },
            new { Metric = "Quality", Score = 92 },
            new { Metric = "Efficiency", Score = 75 },
            new { Metric = "Satisfaction", Score = 85 }
        };

        return new Card().Title("KPI Dashboard with Background")
            | new RadialBarChart(data)
                .ColorScheme(ColorScheme.Default)
                .RadialBar(new RadialBar("Score").Background(true))
                .Tooltip()
                .Legend()
                .Toolbox()
        ;
    }
}

public class RadialBarChart3 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Department = "Engineering", Capacity = 92 },
            new { Department = "Marketing", Capacity = 78 },
            new { Department = "Sales", Capacity = 85 },
            new { Department = "Support", Capacity = 65 }
        };

        return new Card().Title("Department Capacity")
            | new RadialBarChart(data)
                .ColorScheme(ColorScheme.Default)
                .RadialBar("Capacity")
                .InnerRadius("40%")
                .OuterRadius("85%")
                .Tooltip()
                .Legend()
        ;
    }
}

public class RadialBarChart4 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Revenue = 85 },
            new { Month = "Feb", Revenue = 92 },
            new { Month = "Mar", Revenue = 78 },
            new { Month = "Apr", Revenue = 88 },
            new { Month = "May", Revenue = 95 },
            new { Month = "Jun", Revenue = 82 }
        };

        return new Card().Title("Monthly Revenue Gauge")
            | new RadialBarChart(data)
                .ColorScheme(ColorScheme.Default)
                .RadialBar(new RadialBar("Revenue").Background(true).Animated(true))
                .StartAngle(-90)
                .EndAngle(270)
                .InnerRadius("30%")
                .OuterRadius("80%")
                .Tooltip()
                .Toolbox()
        ;
    }
}

public class RadialBarChart5 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Feature = "API", Coverage = 95 },
            new { Feature = "UI", Coverage = 88 },
            new { Feature = "Database", Coverage = 92 },
            new { Feature = "Auth", Coverage = 98 },
            new { Feature = "Integration", Coverage = 75 },
            new { Feature = "Analytics", Coverage = 82 }
        };

        return new Card().Title("Test Coverage by Feature")
            | new RadialBarChart(data)
                .ColorScheme(ColorScheme.Default)
                .RadialBar(new RadialBar("Coverage").Background(true))
                .PolarGrid(new PolarGrid().GridType(PolarGridTypes.Circle))
                .InnerRadius("10%")
                .OuterRadius("90%")
                .Tooltip()
                .Legend()
                .Toolbox()
        ;
    }
}
