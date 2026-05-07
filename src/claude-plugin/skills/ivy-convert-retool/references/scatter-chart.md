# Scatter Chart

Renders data as points on an x-axis and y-axis. Useful for visualizing how data moves between different stages or categories, identifying correlations, and spotting outliers.

## Retool

```toolscript
{{scatterChart1.selectedPoints}}

// Configure via Inspector:
// - Data source: query results mapped to x/y axes
// - Chart type: scatter
// - Axis scale: linear | log | time
// - Legend position: top | right | bottom | left | none
// - Toolbar: zoom, pan, box select, lasso select, autoscale, export
// - Range slider for zooming into data subsets

// Event handler example
scatterChart1.scrollIntoView({ behavior: "smooth", block: "center" });
scatterChart1.setHidden(false);
```

## Ivy

Ivy does not have a dedicated Scatter Chart widget. The closest equivalent is `LineChart` which supports `ReferenceDots` for marking specific data points, configurable axes, legends, toolbox, and tooltips.

```csharp
var data = new[]
{
    new { X = 1, Y = 186 },
    new { X = 2, Y = 305 },
    new { X = 3, Y = 237 },
    new { X = 4, Y = 73 },
    new { X = 5, Y = 209 },
};

// LineChart is the closest available chart type
return Layout.Vertical()
    | data.ToLineChart(style: LineChartStyles.Custom)
        .Dimension("X", e => e.X)
        .Measure("Y", e => e.Sum(f => f.Y))
        .Toolbox()
        .Legend()
        .Tooltip();
```

## Parameters

| Parameter | Retool Documentation | Ivy |
|-----------|---------------------|-----|
| `chartType` | Type of chart to display (`scatter`, `bar`, `line`, etc.) | Not supported (no scatter type; use `LineChart` as closest alternative) |
| `title` | Title text displayed on the chart | Not directly on LineChart (use layout labels) |
| `hidden` | Whether the component is hidden from view | `Visible` (bool) on LineChart |
| `margin` | External spacing around the component (`4px 8px` or `0`) | Not supported (handled by layout) |
| `legendPosition` | Position of the legend (`top`, `right`, `bottom`, `left`, `none`) | `.Legend()` method on LineChart (positioning via `Legend` object) |
| `clearOnEmptyData` | Clears chart when data is empty | Not supported |
| `selectedPoints` | Read-only list of currently selected data points | Not supported |
| `rangeSlider` | Displays a range slider for zooming | Not supported |
| `xAxis.title` | X-axis label text | `XAxis` configuration on LineChart |
| `xAxis.scale` | X-axis scale (`linear`, `log`, `time`) | `Scale` property on LineChart |
| `xAxis.rangeMin` / `rangeMax` | Manual axis boundaries | Not supported |
| `xAxis.rangeMode` | Axis range mode (`auto`, `manual`) | Not supported |
| `xAxis.showGridLines` | Toggle grid lines on x-axis | `CartesianGrid` on LineChart |
| `xAxis.showTickLabels` | Show or hide tick labels | Not supported |
| `xAxis.tickFormat` | Format of axis tick labels | Not supported |
| `xAxis.sort` | Axis sorting (`none`, `ascending`, `descending`) | Not supported |
| `yAxis.title` | Y-axis label text | `YAxis` configuration on LineChart |
| `yAxis.scale` | Y-axis scale (`linear`, `log`, `time`) | `Scale` property on LineChart |
| `yAxis.rangeMin` / `rangeMax` | Manual axis boundaries | Not supported |
| `yAxis.rangeMode` | Axis range mode (`auto`, `manual`) | Not supported |
| `yAxis.showGridLines` | Toggle grid lines on y-axis | `CartesianGrid` on LineChart |
| `yAxis.showTickLabels` | Show or hide tick labels | Not supported |
| `toolbar.showAutoscale` | Show Autoscale button | `.Toolbox()` on LineChart (single toggle) |
| `toolbar.showBoxSelect` | Show Box Select button | Not supported |
| `toolbar.showLassoSelect` | Show Lasso Select button | Not supported |
| `toolbar.showPan` | Show Pan button | `.Toolbox()` on LineChart |
| `toolbar.showZoomIn` | Show Zoom In button | `.Toolbox()` on LineChart |
| `toolbar.showZoomOut` | Show Zoom Out button | `.Toolbox()` on LineChart |
| `toolbar.showZoomSelect` | Show Zoom Select button | `.Toolbox()` on LineChart |
| `toolbar.showResetViews` | Show Reset View button | `.Toolbox()` on LineChart |
| `toolbar.showToImage` | Show Download as PNG button | `.Toolbox()` on LineChart |
| `style` | Custom style options object | `LineChartStyles` (`Default`, `Dashboard`, `Custom`) |
| `events.Select` | Triggered when a data point is selected | Not supported |
| `events.Hover` | Triggered when hovering over a point | `Tooltip` on LineChart (display only) |
| `events.Clear` | Triggered when selection is cleared | Not supported |
| `events.LegendClick` | Triggered when a legend item is clicked | Not supported |
| `scrollIntoView()` | Scrolls component into the visible area | Not supported |
| `setHidden()` | Toggles component visibility | `Visible` property on LineChart |
| `ReferenceDots` | N/A | `ReferenceDots` on LineChart (mark specific data points) |
| `ReferenceLines` | N/A | `ReferenceLines` on LineChart (threshold lines) |
| `ReferenceAreas` | N/A | `ReferenceAreas` on LineChart (highlighted regions) |
| `ColorScheme` | N/A | `ColorScheme` on LineChart |
