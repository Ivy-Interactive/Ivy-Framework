---
searchHints:
  - visualization
  - graph
  - analytics
  - data
  - trends
  - statistics
  - charts
  - sorting
---

# Charts

<Ingress>
Create interactive charts for data visualization using the Chart Builders API.
</Ingress>

## Basic Usage

The simplest way to create a chart is to call a builder method like `.ToLineChart()` on your data. Use `.Dimension()` to define the X-axis grouping and `.Measure()` for Y-axis values with aggregation.

```csharp demo-below
public class BasicChartExample : ViewBase
{
    public override object? Build()
    {
        var salesData = new[]
        {
            new { Month = "Jan", Sales = 186 },
            new { Month = "Feb", Sales = 305 },
            new { Month = "Mar", Sales = 237 },
            new { Month = "Apr", Sales = 289 }
        };

        return salesData.ToLineChart()
            .Dimension("Month", e => e.Month)
            .Measure("Sales", e => e.Sum(f => f.Sales))
            .Toolbox();
    }
}
```

## Array-Based API

For concise chart creation with pre-styled charts, use the array-based API by passing parameters directly to `ToLineChart()` or `ToBarChart()`:

```csharp demo-below
public class ArrayBasedChartExample : ViewBase
{
    public override object? Build()
    {
        var salesData = new[]
        {
            new { Month = "Jan", Sales = 186 },
            new { Month = "Feb", Sales = 305 },
            new { Month = "Mar", Sales = 237 },
            new { Month = "Apr", Sales = 289 }
        };

        // ToLineChart(dimension, measures[], style?)
        return salesData.ToLineChart(
            e => e.Month,
            [e => e.Sum(f => f.Sales)],
            LineChartStyles.Dashboard);
    }
}
```

**Parameter order for LineChart and BarChart:**

1. **dimension** — Expression for X-axis grouping (e.g., `e => e.Month`)
2. **measures[]** — Array of aggregation expressions (e.g., `[e => e.Sum(f => f.Sales)]`)
3. **style** (optional) — Pre-defined styling (e.g., `LineChartStyles.Dashboard`, `BarChartStyles.Dashboard`)

```csharp
// ✅ CORRECT - style comes AFTER measures array
data.ToLineChart(e => e.Date, [e => e.Sum(f => f.Amount)], LineChartStyles.Dashboard)

// ❌ WRONG - style before measures causes CS1503 error
data.ToLineChart(e => e.Date, LineChartStyles.Dashboard, [e => e.Sum(f => f.Amount)])
```

Use this API when you want pre-styled charts with minimal configuration. For custom styling and additional configuration (sorting, toolbox, etc.), use the fluent API shown in "Basic Usage" above.

Ivy supports four chart types — each optimized for different visualization needs:

| Chart | Best For | Builder Method |
|-------|----------|----------------|
| [LineChart](../../02_Widgets/06_Charts/01_LineChart.md) | Trends over time | `.ToLineChart()` |
| [BarChart](../../02_Widgets/06_Charts/02_BarChart.md) | Comparing categories | `.ToBarChart()` |
| [AreaChart](../../02_Widgets/06_Charts/03_AreaChart.md) | Cumulative data | `.ToAreaChart()` |
| [PieChart](../../02_Widgets/06_Charts/04_PieChart.md) | Parts of a whole | `.ToPieChart()` |

## Sorting

By default, chart data appears in the order it exists in your data source. Use `SortBy` to control X-axis ordering.

### Simple Sorting

Sort alphabetically or lexicographically:

```csharp demo-tabs
public class SimpleSortDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Label = "Cherry", Value = 30 },
            new { Label = "Apple", Value = 10 },
            new { Label = "Banana", Value = 20 }
        };

        return Layout.Horizontal().Gap(4)
            | (Layout.Vertical()
                | Text.P("No Sorting").Small()
                | data.ToBarChart()
                    .Dimension("Label", e => e.Label)
                    .Measure("Value", e => e.Sum(f => f.Value)))
            | (Layout.Vertical()
                | Text.P("Ascending").Small()
                | data.ToBarChart()
                    .Dimension("Label", e => e.Label)
                    .Measure("Value", e => e.Sum(f => f.Value))
                    .SortBy(SortOrder.Ascending));
    }
}
```

### Custom Key Sorting

For numeric strings or dates, specify how values should be interpreted:

```csharp demo-tabs
public class CustomSortDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Label = "1", Value = 10 },
            new { Label = "10", Value = 100 },
            new { Label = "2", Value = 20 }
        };

        return Layout.Horizontal().Gap(4)
            | (Layout.Vertical()
                | Text.P("Lexicographic: 1, 10, 2").Small()
                | data.ToLineChart()
                    .Dimension("Label", e => e.Label)
                    .Measure("Value", e => e.Sum(f => f.Value))
                    .SortBy(SortOrder.Ascending))
            | (Layout.Vertical()
                | Text.P("Numeric: 1, 2, 10").Small()
                | data.ToLineChart()
                    .Dimension("Label", e => e.Label)
                    .Measure("Value", e => e.Sum(f => f.Value))
                    .SortBy(e => int.Parse(e.Label), SortOrder.Ascending));
    }
}
```

**SortOrder options:**
- `SortOrder.None` — no sorting (default)
- `SortOrder.Ascending` — A→Z, smallest→largest
- `SortOrder.Descending` — Z→A, largest→smallest

<Callout Type="note">
`SortBy` is available for LineChart, BarChart, and AreaChart. PieChart doesn't use X-axis sorting.
</Callout>

## Styling

### Color Schemes

Control the color palette for chart series:

```csharp demo-tabs
public class ColorSchemeDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", A = 100, B = 80, C = 60 },
            new { Month = "Feb", A = 120, B = 90, C = 70 },
            new { Month = "Mar", A = 140, B = 100, C = 80 }
        };

        return Layout.Horizontal().Gap(4)
            | (Layout.Vertical()
                | Text.P("Default").Small()
                | new BarChart(data)
                    .Bar("A").Bar("B").Bar("C")
                    .ColorScheme(ColorScheme.Default)
                    .Legend())
            | (Layout.Vertical()
                | Text.P("Rainbow").Small()
                | new BarChart(data)
                    .Bar("A").Bar("B").Bar("C")
                    .ColorScheme(ColorScheme.Rainbow)
                    .Legend());
    }
}
```

### Common Methods

All Cartesian charts (Line, Bar, Area) share these methods:

| Method | Description |
|--------|-------------|
| `.CartesianGrid()` | Add grid lines (`.Horizontal()`, `.Vertical()`) |
| `.Stroke()` | Set custom grid line color (e.g., `.Stroke(Colors.Slate)`) |
| `.XAxis()` | Configure X-axis (`.Label<XAxis>("text")`) |
| `.YAxis()` | Configure Y-axis (`.Label<YAxis>("text")`) |
| `.Legend()` | Show legend (`.Layout()`, `.VerticalAlign()`) |
| `.Tooltip()` | Enable hover tooltips |
| `.Toolbox()` | Add interactive toolbox |
| `.Height()` / `.Width()` | Set chart dimensions |

## Advanced Axis Configuration

You can customize the appearance and behavior of X and Y axes using specialized methods.

### Format Tick Labels

Use `.TickFormatter()` to format numeric or date values on the axis using standard C# format strings. This example uses a **Line Chart** to show currency formatting on the Y-axis.

```csharp demo-tabs
public class TickFormatterDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Year = "2020", Revenue = 1500000 },
            new { Year = "2021", Revenue = 2300000 },
            new { Year = "2022", Revenue = 1800000 },
            new { Year = "2023", Revenue = 3100000 }
        };

        return new LineChart(data)
            .Line(new Line("Revenue"))
            .CartesianGrid(new CartesianGrid().Horizontal())
            .XAxis(new XAxis("Year").TickLine(false))
            .YAxis(new YAxis("Revenue").TickFormatter("C0"))
            .Tooltip();
    }
}
```

### Hide Tick Labels

If you want a cleaner look but need to keep the grid lines and axis structure, use `.HideTickLabels()`. This example uses a **Bar Chart** where X-axis labels are hidden for a minimalist appearance.

```csharp demo-tabs
public class HideTickLabelsDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Day = "Mon", Users = 420 },
            new { Day = "Tue", Users = 480 },
            new { Day = "Wed", Users = 510 },
            new { Day = "Thu", Users = 490 },
            new { Day = "Fri", Users = 550 },
            new { Day = "Sat", Users = 620 },
            new { Day = "Sun", Users = 590 }
        };

        return new BarChart(data)
            .Bar(new Bar("Users").Radius(8))
            .CartesianGrid(new CartesianGrid().Horizontal())
            .XAxis(new XAxis("Day").HideTickLabels())
            .YAxis(new YAxis("Users"))
            .Tooltip();
    }
}
```

#### Domain Bounds (Min/Max)

You can explicitly restrict the visible boundaries of an axis using `.Domain()`. This is particularly useful for clipping extreme outlier values. Use `.AllowDataOverflow(true)` to strictly enforce the domain even if data exceeds it. Here is an **Area Chart** demonstrating domain clipping.

```csharp demo-tabs
public class DomainAxisDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Time = "10:00", ServerLoad = 25 },
            new { Time = "10:05", ServerLoad = 30 },
            new { Time = "10:10", ServerLoad = 95 },
            new { Time = "10:15", ServerLoad = 28 },
            new { Time = "10:20", ServerLoad = 22 }
        };

        return new AreaChart(data)
            .Area(new Area("ServerLoad"))
            .CartesianGrid(new CartesianGrid().Horizontal())
            .XAxis(new XAxis("Time"))
            .YAxis(new YAxis("ServerLoad")
                .Domain(0, 50) 
                .AllowDataOverflow(true)
            )
            .Tooltip();
    }
}
```

You can also use symbolic bounds like `AxisDomain.Auto`, `AxisDomain.DataMin`, or `AxisDomain.DataMax`:

```csharp
// Clamp the bottom to zero but let the top auto-scale to data maximum
.YAxis(new YAxis("Revenue")
    .Domain(0, AxisDomain.DataMax))
```

## Faq

<Details>
<Summary>
How do I pass data to a chart
</Summary>
<Body>

Always use the builder pattern extension methods (`.ToLineChart()`, `.ToBarChart()`, `.ToAreaChart()`, `.ToPieChart()`) on your data collection. Do NOT construct charts manually with `List<dynamic>`. Anonymous types work correctly with the builder pattern:

```csharp
var data = new[] { new { Month = "Jan", Sales = 100 } };
return data.ToLineChart()
    .Dimension("Month", e => e.Month)
    .Measure("Sales", e => e.Sum(f => f.Sales));
```

</Body>
</Details>

