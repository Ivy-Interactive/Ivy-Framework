# Chart

A content area to display charts. Supports datasets with x/y values, axis labels, and chart type configuration including cutout percentages for donut-style charts. This component is considered legacy in Retool and will be deprecated in favor of newer Chart components.

## Retool

```toolscript
// Configure a legacy chart with datasets and axis labels
legacyChart1.datasets = [
  {
    data: [186, 305, 237],
    label: "Desktop",
    backgroundColor: "rgba(75, 192, 192, 0.2)",
    borderColor: "rgba(75, 192, 192, 1)"
  },
  {
    data: [100, 200, 300],
    label: "Mobile",
    backgroundColor: "rgba(255, 99, 132, 0.2)",
    borderColor: "rgba(255, 99, 132, 1)"
  }
];
legacyChart1.xValues = ["Jan", "Feb", "Mar"];
legacyChart1.xAxisLabel = "Month";
legacyChart1.yAxisLabel = "Visitors";
```

## Ivy

Ivy does not have a single unified "Chart" widget. Instead, it provides dedicated chart types: `BarChart`, `LineChart`, `AreaChart`, and `PieChart`. Each is created via a builder extension method on a data collection.

```csharp
// BarChart — closest equivalent to Retool's legacy Chart
var data = new[]
{
    new { Month = "Jan", Desktop = 186, Mobile = 100 },
    new { Month = "Feb", Desktop = 305, Mobile = 200 },
    new { Month = "Mar", Desktop = 237, Mobile = 300 },
};

return data.ToBarChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .Measure("Mobile", e => e.Sum(f => f.Mobile))
    .XAxis(new XAxis().Label("Month"))
    .YAxis(new YAxis().Label("Visitors"))
    .Legend()
    .Tooltip()
    .Toolbox();
```

```csharp
// LineChart
data.ToLineChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .Measure("Mobile", e => e.Sum(f => f.Mobile))
    .CartesianGrid()
    .Tooltip();
```

```csharp
// PieChart (donut via InnerRadius — equivalent to Retool's cutoutPercentage)
data.ToPieChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .Pie(new Pie().InnerRadius(60).OuterRadius(100))
    .Legend()
    .Tooltip();
```

```csharp
// AreaChart
data.ToAreaChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .Measure("Mobile", e => e.Sum(f => f.Mobile))
    .CartesianGrid()
    .Tooltip()
    .Legend();
```

## Parameters

| Parameter              | Retool Documentation                                                        | Ivy                                                                                               |
|------------------------|-----------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------|
| `datasets`             | Array of dataset objects with `data`, `label`, colors, etc.                 | `.Measure("name", aggregation)` per series; data passed to `.ToBarChart()` / `.ToLineChart()` etc. |
| `xValues`              | Array of x-axis string/number values                                        | `.Dimension("name", e => e.Property)` — derived from data via lambda                              |
| `xAxisLabel`           | Label text for the x-axis                                                   | `.XAxis(new XAxis().Label("text"))`                                                                |
| `yAxisLabel`           | Label text for the y-axis                                                   | `.YAxis(new YAxis().Label("text"))`                                                                |
| `xValuesType`          | Type of x-axis values (`"number"` or `"string"`)                            | Not supported (inferred from data)                                                                 |
| `autoSkipTicks`        | Whether to skip ticks that are not evenly spaced                            | Not supported                                                                                      |
| `cutoutPercentage`     | Percentage cut from center (donut charts)                                   | `.Pie(new Pie().InnerRadius(n))` on `PieChart`                                                     |
| `hidden`               | Whether the component is hidden                                             | `.Visible` property                                                                                |
| `id`                   | Unique component identifier                                                 | Not applicable (C# variable reference)                                                             |
| `margin`               | Outer spacing (`"4px 8px"` or `"0"`)                                        | Not supported (handled by layout)                                                                  |
| `style`                | Custom CSS style object                                                     | `.ColorScheme()`, `.Height()`, `.Width()`                                                          |
| `isHiddenOnDesktop`    | Hide on desktop layout                                                      | Not supported                                                                                      |
| `isHiddenOnMobile`     | Hide on mobile layout                                                       | Not supported                                                                                      |
| `maintainSpaceWhenHidden` | Reserve space when hidden                                                | Not supported                                                                                      |
| `showInEditor`         | Show in editor when hidden                                                  | Not supported                                                                                      |
| Events: `Clear`        | Triggered when a value is cleared                                           | Not supported                                                                                      |
| Events: `Select`       | Triggered when a value is selected                                          | Not supported                                                                                      |
| —                      | —                                                                           | `.CartesianGrid()` — grid lines (Ivy only)                                                         |
| —                      | —                                                                           | `.Toolbox()` — interactive controls (Ivy only)                                                     |
| —                      | —                                                                           | `.Tooltip()` — hover information (Ivy only)                                                        |
| —                      | —                                                                           | `.Legend()` — series legend (Ivy only)                                                              |
| —                      | —                                                                           | `.SortBy(SortOrder)` — axis sorting (Ivy only)                                                     |
| —                      | —                                                                           | `.ReferenceLines()` / `.ReferenceDots()` / `.ReferenceAreas()` (Ivy only)                          |
