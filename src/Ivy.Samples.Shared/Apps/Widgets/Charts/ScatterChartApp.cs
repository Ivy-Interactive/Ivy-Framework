
namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.ChartScatter, searchHints: ["visualization", "scatter", "bubble", "correlation", "analytics", "data", "points"])]
public class ScatterChartApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Grid().Columns(3)
            | new ScatterChart0View()
            | new ScatterChart1View()
            | new ScatterChart2View()
            | new ScatterChart3View()
            | new ScatterChart4View()
            | new ScatterChart5View()
            | new ScatterChart6View()
            | new ScatterChart7View()
            | new ScatterChart8View()
            | new ScatterChart9View()
            | new ScatterChart10View()
            | new ScatterChart11View()
        ;
    }
}

// Example 1: Basic Scatter Plot (Height vs Weight)
public class ScatterChart0View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Height = 165, Weight = 65, Age = 25, Gender = "Male" },
            new { Height = 170, Weight = 72, Age = 30, Gender = "Male" },
            new { Height = 158, Weight = 58, Age = 28, Gender = "Female" },
            new { Height = 175, Weight = 78, Age = 35, Gender = "Male" },
            new { Height = 162, Weight = 60, Age = 22, Gender = "Female" },
            new { Height = 180, Weight = 85, Age = 40, Gender = "Male" },
            new { Height = 155, Weight = 52, Age = 20, Gender = "Female" },
            new { Height = 168, Weight = 68, Age = 27, Gender = "Male" },
            new { Height = 160, Weight = 62, Age = 24, Gender = "Female" },
            new { Height = 172, Weight = 75, Age = 32, Gender = "Male" },
        };

        var chart = new ScatterChart(data)
            .Scatter(new Scatter("Value").Name("Height vs Weight"))
            .XAxis(new XAxis("Height").Type(AxisTypes.Number))
            .YAxis(new YAxis("Weight").Type(AxisTypes.Number))
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend();

        return new Card().Title("Basic Scatter Plot (Height vs Weight)")
            | chart;
    }
}

// Example 2: Bubble Chart with Size Encoding (Age as bubble size)
public class ScatterChart1View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Height = 165, Weight = 65, Age = 25, Gender = "Male" },
            new { Height = 170, Weight = 72, Age = 45, Gender = "Male" },
            new { Height = 158, Weight = 58, Age = 22, Gender = "Female" },
            new { Height = 175, Weight = 78, Age = 50, Gender = "Male" },
            new { Height = 162, Weight = 60, Age = 20, Gender = "Female" },
            new { Height = 180, Weight = 85, Age = 55, Gender = "Male" },
            new { Height = 155, Weight = 52, Age = 18, Gender = "Female" },
            new { Height = 168, Weight = 68, Age = 35, Gender = "Male" },
            new { Height = 160, Weight = 62, Age = 28, Gender = "Female" },
            new { Height = 172, Weight = 75, Age = 42, Gender = "Male" },
        };

        var chart = new ScatterChart(data)
            .Scatter(new Scatter("Value").Name("People"))
            .XAxis(new XAxis("Height").Type(AxisTypes.Number))
            .YAxis(new YAxis("Weight").Type(AxisTypes.Number))
            .ZAxis(new ZAxis("Age").Range(40, 200))
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend()
            .CartesianGrid(new CartesianGrid().Horizontal().Vertical());

        return new Card().Title("Bubble Chart (Age as Size)")
            | chart;
    }
}

// Example 3: Multiple Series with Different Shapes
public class ScatterChart2View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { X = 10, Y = 20, Series = "A" },
            new { X = 15, Y = 25, Series = "A" },
            new { X = 20, Y = 30, Series = "A" },
            new { X = 25, Y = 35, Series = "A" },
            new { X = 12, Y = 18, Series = "B" },
            new { X = 18, Y = 22, Series = "B" },
            new { X = 24, Y = 28, Series = "B" },
            new { X = 30, Y = 32, Series = "B" },
        };

        var chart = new ScatterChart(data)
            .Scatter(new Scatter("Value").Name("Series A").Shape(ScatterShape.Circle).Fill(Colors.Blue))
            .Scatter(new Scatter("Value").Name("Series B").Shape(ScatterShape.Diamond).Fill(Colors.Red))
            .XAxis(new XAxis("X").Type(AxisTypes.Number))
            .YAxis(new YAxis("Y").Type(AxisTypes.Number))
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend()
            .CartesianGrid();

        return new Card().Title("Multiple Series (Different Shapes)")
            | chart;
    }
}

// Example 4: Connected Scatter Plot (Joint Interpolation)
public class ScatterChart3View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Time = 1, Temperature = 20, Humidity = 65 },
            new { Time = 2, Temperature = 22, Humidity = 62 },
            new { Time = 3, Temperature = 24, Humidity = 58 },
            new { Time = 4, Temperature = 26, Humidity = 55 },
            new { Time = 5, Temperature = 28, Humidity = 52 },
            new { Time = 6, Temperature = 30, Humidity = 48 },
        };

        var chart = new ScatterChart(data)
            .Scatter(new Scatter("Value").Name("Temperature vs Humidity").Line(true).LineType(ScatterLineType.Joint))
            .XAxis(new XAxis("Temperature").Type(AxisTypes.Number))
            .YAxis(new YAxis("Humidity").Type(AxisTypes.Number))
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend()
            .CartesianGrid(new CartesianGrid().Horizontal().Vertical());

        return new Card().Title("Connected Scatter (Joint)")
            | chart;
    }
}

// Example 5: Connected Scatter with Smooth Fitting
public class ScatterChart4View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { X = 5, Y = 10 },
            new { X = 10, Y = 22 },
            new { X = 15, Y = 18 },
            new { X = 20, Y = 35 },
            new { X = 25, Y = 30 },
            new { X = 30, Y = 48 },
            new { X = 35, Y = 42 },
            new { X = 40, Y = 58 },
        };

        var chart = new ScatterChart(data)
            .Scatter(new Scatter("Value").Name("Smooth Trend").Line(true).LineType(ScatterLineType.Fitting))
            .XAxis(new XAxis("X").Type(AxisTypes.Number))
            .YAxis(new YAxis("Y").Type(AxisTypes.Number))
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend()
            .CartesianGrid();

        return new Card().Title("Connected Scatter (Fitting)")
            | chart;
    }
}

// Example 6: Custom ZAxis Range Configuration
public class ScatterChart5View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Income = 30000, Savings = 5000, Expenses = 25000 },
            new { Income = 45000, Savings = 10000, Expenses = 35000 },
            new { Income = 60000, Savings = 18000, Expenses = 42000 },
            new { Income = 75000, Savings = 25000, Expenses = 50000 },
            new { Income = 90000, Savings = 35000, Expenses = 55000 },
            new { Income = 105000, Savings = 45000, Expenses = 60000 },
        };

        var chart = new ScatterChart(data)
            .Scatter(new Scatter("Value").Name("Financial Data"))
            .XAxis(new XAxis("Income").Type(AxisTypes.Number))
            .YAxis(new YAxis("Savings").Type(AxisTypes.Number))
            .ZAxis(new ZAxis("Expenses").Range(50, 300))
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend()
            .CartesianGrid(new CartesianGrid().Horizontal().Vertical());

        return new Card().Title("Custom ZAxis Range (50-300)")
            | chart;
    }
}

// Example 7: Scatter with Reference Lines
public class ScatterChart6View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Score1 = 65, Score2 = 70 },
            new { Score1 = 72, Score2 = 75 },
            new { Score1 = 78, Score2 = 82 },
            new { Score1 = 85, Score2 = 88 },
            new { Score1 = 90, Score2 = 92 },
            new { Score1 = 55, Score2 = 60 },
            new { Score1 = 68, Score2 = 72 },
            new { Score1 = 82, Score2 = 85 },
        };

        var chart = new ScatterChart(data)
            .Scatter(new Scatter("Value").Name("Test Scores"))
            .XAxis(new XAxis("Score1").Type(AxisTypes.Number))
            .YAxis(new YAxis("Score2").Type(AxisTypes.Number))
            .ReferenceLine(75, null, "Passing Score (X)")
            .ReferenceLine(null, 75, "Passing Score (Y)")
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend()
            .CartesianGrid();

        return new Card().Title("Scatter with Reference Lines")
            | chart;
    }
}

// Example 8: Scatter with Reference Area
public class ScatterChart7View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { X = 10, Y = 15 },
            new { X = 20, Y = 25 },
            new { X = 30, Y = 35 },
            new { X = 40, Y = 45 },
            new { X = 50, Y = 55 },
            new { X = 60, Y = 65 },
            new { X = 70, Y = 75 },
            new { X = 80, Y = 85 },
        };

        var chart = new ScatterChart(data)
            .Scatter(new Scatter("Value").Name("Data Points"))
            .XAxis(new XAxis("X").Type(AxisTypes.Number))
            .YAxis(new YAxis("Y").Type(AxisTypes.Number))
            .ReferenceArea(30, 30, 60, 60, "Target Zone")
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend()
            .CartesianGrid(new CartesianGrid().Horizontal().Vertical());

        return new Card().Title("Scatter with Reference Area")
            | chart;
    }
}

// Example 9: Dashboard Style Scatter
public class ScatterChart8View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { CPU = 25, Memory = 30, Load = 500 },
            new { CPU = 35, Memory = 45, Load = 750 },
            new { CPU = 45, Memory = 55, Load = 1000 },
            new { CPU = 55, Memory = 65, Load = 1250 },
            new { CPU = 65, Memory = 75, Load = 1500 },
            new { CPU = 75, Memory = 85, Load = 1750 },
        };

        var chart = new ScatterChart(data)
            .ColorScheme(ColorScheme.Default)
            .Scatter(new Scatter("Value").Name("Server Metrics"))
            .XAxis(new XAxis("CPU").Type(AxisTypes.Number).TickLine(false).AxisLine(false))
            .YAxis(new YAxis("Memory").Type(AxisTypes.Number).TickLine(false).AxisLine(false))
            .ZAxis(new ZAxis("Load").Range(60, 200))
            .Tooltip(new ChartTooltip().Animated(true))
            .CartesianGrid(new CartesianGrid().Horizontal().Vertical());

        return new Card().Title("Dashboard Style (Server Metrics)")
            | chart;
    }
}

// Example 10: Custom Style with Diamond Shapes
public class ScatterChart9View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { X = 15, Y = 20 },
            new { X = 25, Y = 35 },
            new { X = 35, Y = 30 },
            new { X = 45, Y = 50 },
            new { X = 55, Y = 45 },
            new { X = 65, Y = 65 },
            new { X = 75, Y = 70 },
        };

        var chart = new ScatterChart(data)
            .ColorScheme(ColorScheme.Rainbow)
            .Scatter(new Scatter("Value").Name("Custom Shape").Shape(ScatterShape.Diamond))
            .XAxis(new XAxis("X").Type(AxisTypes.Number).TickLine(true).AxisLine(true))
            .YAxis(new YAxis("Y").Type(AxisTypes.Number).TickLine(true).AxisLine(true))
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend()
            .CartesianGrid(new CartesianGrid().Horizontal().Vertical());

        return new Card().Title("Custom Style (Diamond Shapes)")
            | chart;
    }
}

// Example 11: All Available Shapes Showcase
public class ScatterChart10View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { X = 10, Y = 10, Shape = "Circle" },
            new { X = 20, Y = 20, Shape = "Square" },
            new { X = 30, Y = 30, Shape = "Cross" },
            new { X = 40, Y = 40, Shape = "Diamond" },
            new { X = 50, Y = 50, Shape = "Star" },
            new { X = 60, Y = 60, Shape = "Triangle" },
            new { X = 70, Y = 70, Shape = "Wye" },
        };

        var chart = new ScatterChart(data)
            .Scatter(new Scatter("Value").Name("Circle").Shape(ScatterShape.Circle))
            .Scatter(new Scatter("Value").Name("Square").Shape(ScatterShape.Square))
            .Scatter(new Scatter("Value").Name("Diamond").Shape(ScatterShape.Diamond))
            .Scatter(new Scatter("Value").Name("Triangle").Shape(ScatterShape.Triangle))
            .XAxis(new XAxis("X").Type(AxisTypes.Number))
            .YAxis(new YAxis("Y").Type(AxisTypes.Number))
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend()
            .CartesianGrid();

        return new Card().Title("All Scatter Shapes")
            | chart;
    }
}

// Example 12: Scatter with Toolbox Features
public class ScatterChart11View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { X = 5, Y = 10, Size = 100 },
            new { X = 15, Y = 25, Size = 200 },
            new { X = 25, Y = 20, Size = 150 },
            new { X = 35, Y = 40, Size = 300 },
            new { X = 45, Y = 35, Size = 250 },
            new { X = 55, Y = 55, Size = 400 },
            new { X = 65, Y = 50, Size = 350 },
            new { X = 75, Y = 70, Size = 500 },
        };

        var chart = new ScatterChart(data)
            .Scatter(new Scatter("Value").Name("Interactive Data"))
            .XAxis(new XAxis("X").Type(AxisTypes.Number))
            .YAxis(new YAxis("Y").Type(AxisTypes.Number))
            .ZAxis(new ZAxis("Size").Range(60, 250))
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend()
            .Toolbox(new Toolbox())
            .CartesianGrid(new CartesianGrid().Horizontal().Vertical());

        return new Card().Title("Scatter with Toolbox (Save, Zoom, etc.)")
            | chart;
    }
}
