---
searchHints:
  - visualization
  - scatter
  - bubble
  - correlation
  - analytics
  - data
  - points
  - distribution
---

# ScatterChart

<Ingress>
Visualize correlations and distributions with interactive scatter plots that support bubble charts, multiple series, and customizable point shapes.
</Ingress>

Scatter charts display data points on a two-dimensional coordinate system, making them ideal for showing correlations between two numeric variables. Build chart [views](../../01_Onboarding/02_Concepts/02_Views.md) inside [layouts](../../01_Onboarding/02_Concepts/04_Layout.md) and use [state](../../03_Hooks/02_Core/03_UseState.md) for dynamic data. See [Charts](../../01_Onboarding/02_Concepts/18_Charts.md) for an overview of Ivy chart widgets. The example below renders a basic scatter plot showing the relationship between height and weight.

```csharp demo-tabs

public class BasicScatterChartDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Height = 165, Weight = 65 },
            new { Height = 170, Weight = 72 },
            new { Height = 158, Weight = 58 },
            new { Height = 175, Weight = 78 },
            new { Height = 162, Weight = 60 },
            new { Height = 180, Weight = 85 },
            new { Height = 155, Weight = 52 },
            new { Height = 168, Weight = 68 },
        };

        return Layout.Vertical()
            | new ScatterChart(data)
                .Scatter(new Scatter("Value").Name("Height vs Weight"))
                .XAxis(new XAxis("Height").Type(AxisTypes.Number))
                .YAxis(new YAxis("Weight").Type(AxisTypes.Number))
                .Tooltip(new ChartTooltip().Animated(true))
                .Legend()
                .CartesianGrid();
    }
}
```

## Bubble Charts

Scatter charts can encode a third dimension using bubble size through the `ZAxis` property. This creates bubble charts where point size represents an additional numeric value:

```csharp demo-tabs
public class BubbleChartDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Height = 165, Weight = 65, Age = 25 },
            new { Height = 170, Weight = 72, Age = 45 },
            new { Height = 158, Weight = 58, Age = 22 },
            new { Height = 175, Weight = 78, Age = 50 },
            new { Height = 162, Weight = 60, Age = 20 },
            new { Height = 180, Weight = 85, Age = 55 },
            new { Height = 155, Weight = 52, Age = 18 },
            new { Height = 168, Weight = 68, Age = 35 },
        };

        return Layout.Vertical()
            | new ScatterChart(data)
                .Scatter(new Scatter("Value").Name("People"))
                .XAxis(new XAxis("Height").Type(AxisTypes.Number))
                .YAxis(new YAxis("Weight").Type(AxisTypes.Number))
                .ZAxis(new ZAxis("Age").Range(40, 200))
                .Tooltip(new ChartTooltip().Animated(true))
                .Legend()
                .CartesianGrid(new CartesianGrid().Horizontal().Vertical());
    }
}
```

## Point Shapes

Customize point appearance with different shapes. Available shapes include `Circle`, `Square`, `Cross`, `Diamond`, `Star`, `Triangle`, and `Wye`:

```csharp demo-tabs
public class ScatterShapesDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { X = 10, Y = 20 },
            new { X = 15, Y = 25 },
            new { X = 20, Y = 30 },
            new { X = 25, Y = 35 },
            new { X = 30, Y = 40 },
        };

        return Layout.Vertical()
            | new ScatterChart(data)
                .Scatter(new Scatter("Value")
                    .Name("Diamond Points")
                    .Shape(ScatterShape.Diamond)
                    .Fill(Colors.Blue))
                .XAxis(new XAxis("X").Type(AxisTypes.Number))
                .YAxis(new YAxis("Y").Type(AxisTypes.Number))
                .Tooltip(new ChartTooltip().Animated(true))
                .Legend()
                .CartesianGrid();
    }
}
```

## Connected Scatter

Connect scatter points with lines to show sequential relationships. Use `Joint` for straight connections or `Fitting` for smooth interpolation:

```csharp demo-tabs
public class ConnectedScatterDemo : ViewBase
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

        return Layout.Vertical()
            | new ScatterChart(data)
                .Scatter(new Scatter("Value")
                    .Name("Temperature vs Humidity")
                    .Line(true)
                    .LineType(ScatterLineType.Fitting))
                .XAxis(new XAxis("Temperature").Type(AxisTypes.Number))
                .YAxis(new YAxis("Humidity").Type(AxisTypes.Number))
                .Tooltip(new ChartTooltip().Animated(true))
                .Legend()
                .CartesianGrid(new CartesianGrid().Horizontal().Vertical());
    }
}
```

## ZAxis Configuration

Control bubble size range with the `Range` method on `ZAxis`. The range determines the minimum and maximum pixel sizes for bubbles:

```csharp demo-tabs
public class ZAxisRangeDemo : ViewBase
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

        return Layout.Vertical()
            | new ScatterChart(data)
                .Scatter(new Scatter("Value").Name("Financial Data"))
                .XAxis(new XAxis("Income").Type(AxisTypes.Number))
                .YAxis(new YAxis("Savings").Type(AxisTypes.Number))
                .ZAxis(new ZAxis("Expenses").Range(50, 300))
                .Tooltip(new ChartTooltip().Animated(true))
                .Legend()
                .CartesianGrid(new CartesianGrid().Horizontal().Vertical());
    }
}
```

<WidgetDocs Type="Ivy.ScatterChart" ExtensionTypes="Ivy.ScatterChartExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Charts/ScatterChart.cs"/>

## Example

<Details>
<Summary>
Correlation Analysis
</Summary>
<Body>
ScatterChart can be used to analyze correlations between variables. The following example demonstrates a multi-dimensional analysis with size encoding:

```csharp demo-tabs
public class CorrelationAnalysisDemo : ViewBase
{
    public override object? Build()
    {
        var random = new Random(42); // Fixed seed for consistent data

        var sampleData = Enumerable.Range(1, 50)
            .Select(i => new {
                StudyHours = random.Next(5, 50),
                TestScore = random.Next(40, 100),
                AttendancePercent = random.Next(60, 100)
            })
            .ToArray();

        return Layout.Vertical()
            | Text.P("Student Performance Analysis").Large()
            | Text.P($"Analyzing {sampleData.Length} students").Small()
            | Text.Html("<i>Study Hours vs Test Score (bubble size = attendance %)</i>")
            | new ScatterChart(sampleData)
                .Scatter(new Scatter("Value").Name("Students"))
                .XAxis(new XAxis("StudyHours").Type(AxisTypes.Number))
                .YAxis(new YAxis("TestScore").Type(AxisTypes.Number))
                .ZAxis(new ZAxis("AttendancePercent").Range(60, 250))
                .Tooltip(new ChartTooltip().Animated(true))
                .Legend()
                .Toolbox(new Toolbox())
                .CartesianGrid(new CartesianGrid().Horizontal().Vertical());
    }
}
```

</Body>
</Details>
