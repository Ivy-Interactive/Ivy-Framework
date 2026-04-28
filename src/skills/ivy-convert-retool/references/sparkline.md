# Sparkline

A simplified line chart component for visualizing data trends over time. Renders data as a line on an x-axis and y-axis, useful for compact trend displays. It is a streamlined version of the full Line Chart component.

## Retool

```toolscript
// Sparkline with basic configuration
sparkline1.chartType = "line";
sparkline1.lineColor = "#4A90D9";
sparkline1.lineShape = "spline";
sparkline1.showMarkers = false;
sparkline1.legendPosition = "none";
sparkline1.title = "Revenue Trend";

// Data source (typically bound to a query)
sparkline1.datasource = [
  { x: "Jan", y: 100 },
  { x: "Feb", y: 150 },
  { x: "Mar", y: 130 },
  { x: "Apr", y: 180 },
  { x: "May", y: 200 }
];
```

## Ivy

```csharp
// Ivy uses LineChart as the equivalent of Sparkline
var data = new[]
{
    new { Month = "Jan", Revenue = 100 },
    new { Month = "Feb", Revenue = 150 },
    new { Month = "Mar", Revenue = 130 },
    new { Month = "Apr", Revenue = 180 },
    new { Month = "May", Revenue = 200 }
};

new LineChart(data, dataKey: "Month", nameKey: "Revenue");

// Or using the fluent extension method with styles
data.ToLineChart(style: LineChartStyles.Default);

// Custom stroke width
data.ToLineChart(style: LineChartStyles.Custom).StrokeWidth(2);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `chartType` | Type of chart to display (line, bar, pie, etc.) | Not supported (dedicated `LineChart` widget) |
| `lineColor` | Color of the line (`#000000`) | `ColorScheme` on `LineChart` |
| `lineWidth` | Width of the line | `.StrokeWidth(int)` |
| `lineShape` | Shape of the line (linear, spline, hv, vh, hvh, vhv) | `LineChartStyles.Default` (spline) / `LineChartStyles.Custom` (linear) |
| `lineDash` | Line style: solid, dash, dot | Not supported |
| `showMarkers` | Whether to show markers for data points | Not supported |
| `markerColor` | Color of the marker | Not supported |
| `markerSymbol` | Marker type: circle, square, diamond | Not supported |
| `title` | Chart title text | Not supported |
| `hidden` | Hide component from view | `Visible` (bool) |
| `datasource` | Source data array | `Data` (object) via constructor |
| `legendPosition` | Legend placement (top, right, bottom, left, none) | `.Legend()` via `Legend` property |
| `hoverTemplate` | Tooltip template on hover | `Tooltip` property |
| `xAxis` | X-axis configuration (range, scale, grid lines, tick format) | `XAxis` property (`XAxis[]`) |
| `yAxis` | Y-axis configuration (range, scale, grid lines, tick format) | `YAxis` property (`YAxis[]`) |
| `rangeSlider` | Display a range slider for zooming | Not supported |
| `aggregationType` | Aggregation: sum, average, count, min, max, etc. | `.Measure()` with custom aggregation |
| `groupBy` | Column to group data by | `.Dimension()` method |
| `barMode` | Bar mode: stack, group, overlay, relative | Not supported (line chart only) |
| `barOrientation` | Bar orientation: vertical, horizontal | Not supported (line chart only) |
| `toolbar` | Toolbar buttons (zoom, pan, autoscale, download PNG, etc.) | `Toolbox` property via `.Toolbox()` |
| `clearOnEmptyData` | Clear chart when data is empty | Not supported |
| `selectedPoints` | List of selected data points (read-only) | Not supported |
| `series` | Chart series data with per-series styling | `Lines` property (`Line[]`) |
| `showSecondYAxis` | Show a second Y-axis | Multiple `YAxis[]` entries |
| `margin` | Outer margin spacing | Not supported |
| `maintainSpaceWhenHidden` | Reserve space when hidden | Not supported |
| `isHiddenOnMobile` | Hide on mobile layout | Not supported |
| `isHiddenOnDesktop` | Hide on desktop layout | Not supported |
| `CartesianGrid` | N/A | `CartesianGrid` property (Ivy only) |
| `ReferenceLines` | N/A | `ReferenceLines` property (Ivy only) |
| `ReferenceAreas` | N/A | `ReferenceAreas` property (Ivy only) |
| `ReferenceDots` | N/A | `ReferenceDots` property (Ivy only) |
| `Layout` | N/A | `Layout` property (Ivy only) |
| `Scale` | N/A | `Scale` property (Ivy only) |
