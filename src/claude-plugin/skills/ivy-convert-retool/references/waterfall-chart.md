# Waterfall Chart

Displays data as a series of steps with sequential positive and negative values, visualizing how incremental amounts contribute to a total (e.g. revenue breakdowns, profit analysis).

## Retool

```toolscript
{{
  waterfallChart1.chartType = "waterfall";
  waterfallChart1.series = {
    x: ["Revenue", "Cost of Sales", "Gross Profit", "Operating Expenses", "Net Profit"],
    y: [500, -200, 300, -150, 150]
  };
  waterfallChart1.title = "Profit Waterfall";
  waterfallChart1.yAxis.showGridLines = true;
  waterfallChart1.legendPosition = "bottom";
  waterfallChart1.hoverTemplate = "%{value}<br>%{percent}<extra></extra>";
}}
```

## Ivy

```csharp
// Ivy does not have a dedicated Waterfall Chart widget.
// The closest alternative is BarChart, which can represent
// positive/negative values but lacks native waterfall
// connectors and running totals.

var data = new[]
{
    new { Category = "Revenue",            Value = 500 },
    new { Category = "Cost of Sales",      Value = -200 },
    new { Category = "Gross Profit",       Value = 300 },
    new { Category = "Operating Expenses", Value = -150 },
    new { Category = "Net Profit",         Value = 150 },
};

return data.ToBarChart()
    .Dimension("Category", e => e.Category)
    .Measure("Value", e => e.Sum(f => f.Value))
    .Toolbox();
```

## Parameters

| Parameter           | Documentation                                                                 | Ivy                                                    |
|---------------------|-------------------------------------------------------------------------------|--------------------------------------------------------|
| chartType           | Set to `"waterfall"` to display waterfall chart                               | Not supported (no waterfall type)                      |
| series              | Object containing x and y axis data arrays                                    | `Data` property + `.Dimension()` / `.Measure()` API   |
| datasource          | Source data when using UI Form (array)                                         | `Data` property on BarChart constructor                |
| title               | Chart title text                                                              | Not directly on chart; use layout labels               |
| xAxis               | Object controlling x-axis appearance, scale, and labels                       | `XAxis` property                                       |
| yAxis               | Object controlling y-axis appearance, scale, and labels                       | `YAxis` property                                       |
| rangeMin / rangeMax | Manual axis range settings                                                    | Not supported                                          |
| scale               | Linear, logarithmic, or time-based axis scale                                 | `Scale` property                                       |
| showGridLines       | Toggle grid lines on axes                                                     | `CartesianGrid` property + `.Horizontal()`             |
| showLine            | Toggle axis line visibility                                                   | `.AxisLine(bool)`                                      |
| showTickLabels      | Toggle tick labels on axes                                                    | `.TickLine(bool)`                                      |
| showZeroLine        | Toggle zero reference line                                                    | `ReferenceLines` (manual config)                       |
| tickFormat          | Format string for axis tick labels                                            | Not supported                                          |
| dataLabelTemplate   | Template for data labels displayed on bars                                    | Not supported                                          |
| dataLabelPosition   | Position of data labels (`"top"` or `"left"`)                                 | Not supported                                          |
| aggregationType     | Aggregation: sum, average, count, max, min, median, mode, stddev             | Via `.Measure()` lambda (e.g. `e.Sum(...)`)            |
| hoverTemplate       | Tooltip display template                                                      | `Tooltip` property                                     |
| lineColor           | Color of connector lines (default `"#000000"`)                                | Not supported (no waterfall connectors)                |
| lineDash            | Connector line style: solid, dash, dot                                        | Not supported                                          |
| lineShape           | Connector line shape: linear, spline, etc.                                    | Not supported                                          |
| markerColor         | Color of data point markers                                                   | `.Fill(Colors)` on bars                                |
| markerBorderColor   | Border color of markers                                                       | Not supported                                          |
| markerSymbol        | Symbol shape for markers                                                      | Not supported                                          |
| legendPosition      | Legend placement: top, right, bottom, left, none                              | `Legend` property + `.LegendType()`                    |
| hidden              | Toggle component visibility                                                   | `Visible` property                                     |
| rangeSlider         | Display a range slider below chart                                            | Not supported                                          |
| toolbar             | Configure toolbar buttons (autoscale, pan, zoom, download)                    | `Toolbox` property via `.Toolbox()`                    |
| scrollIntoView()    | Method to scroll component into view                                          | Not supported                                          |
| setHidden()         | Method to toggle visibility programmatically                                  | `Visible` property                                     |
