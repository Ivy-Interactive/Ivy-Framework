# Mixed Chart

Renders data on an x-axis and multiple y-axes, combining several different chart types (bar, line, area, scatter, etc.) into a single visualization. Useful for visualizing the relationship between different datasets.

## Retool

```toolscript
// Mixed Chart component configured via the Inspector UI.
// Data is typically bound from a query result:
mixedChart1.chartType = "mixed";
mixedChart1.barMode = "stack";
mixedChart1.barOrientation = "vertical";
mixedChart1.legendPosition = "bottom";
mixedChart1.xAxis.title = "Month";
mixedChart1.yAxis.title = "Revenue";
mixedChart1.yAxis.scale = "linear";
mixedChart1.showSecondYAxis = true;
mixedChart1.yAxis2.title = "Users";

// Toolbar options
mixedChart1.toolbar.showZoomIn = true;
mixedChart1.toolbar.showZoomOut = true;
mixedChart1.toolbar.showResetViews = true;
mixedChart1.toolbar.showToImage = true;

// Event handlers
mixedChart1.events = [{ event: "Select", type: "script", method: "handleSelect" }];
```

## Ivy

Ivy does not have a single Mixed Chart widget. Instead, use separate `LineChart`, `BarChart`, and `AreaChart` widgets composed together. Each chart type supports multiple series, dual Y-axes, toolbox, and legends independently.

```csharp
// Bar + Line equivalent: use individual chart widgets side by side,
// or use a BarChart/LineChart with multiple series on shared axes.

var data = new[]
{
    new { Month = "Jan", Revenue = 4200, Users = 186, Growth = 12.5 },
    new { Month = "Feb", Revenue = 5800, Users = 305, Growth = 18.2 },
    new { Month = "Mar", Revenue = 5100, Users = 237, Growth = 15.1 },
};

// BarChart with multiple measures
var barChart = data.ToBarChart()
    .Dimension("Month", e => e.Month)
    .Measure("Revenue", e => e.Sum(f => f.Revenue))
    .Measure("Users", e => e.Sum(f => f.Users))
    .Legend()
    .Toolbox();

// LineChart with multiple series
var lineChart = data.ToLineChart()
    .Dimension("Month", e => e.Month)
    .Measure("Revenue", e => e.Sum(f => f.Revenue))
    .Measure("Growth", e => e.Sum(f => f.Growth))
    .Legend()
    .Toolbox();
```

## Parameters

| Parameter | Retool Documentation | Ivy |
|-----------|---------------------|-----|
| `chartType` | Type of chart to display (bar, line, scatter, pie, mixed, bubble, funnel, heatmap, etc.) | Separate widgets: `BarChart`, `LineChart`, `AreaChart`, `PieChart` |
| `barGap` | Gap between bars relative to bar size (0-1, default 0.4) | `BarChart.BarGap()` |
| `barGroupGap` | Gap between bars in a group (0-1, default 0) | `BarChart.BarCategoryGap()` |
| `barMode` | Bar mode: stack, group, overlay, relative (default stack) | `BarChart.StackOffset()` / `BarChart.ReverseStackOrder()` |
| `barOrientation` | Vertical or horizontal bar orientation | `BarChart.Layout()` (Layouts enum) |
| `title` | Chart title text | Not supported (use external label) |
| `hidden` | Whether component is hidden | `Visible` property on all chart widgets |
| `legendPosition` | Legend position: top, right, bottom, left, none | `.Legend()` on all chart types |
| `rangeSlider` | Display a range slider for zooming | Not supported |
| `clearOnEmptyData` | Clear chart when data is empty | Not supported |
| `selectedPoints` | Array of selected data points (read-only) | Not supported |
| `showSecondYAxis` | Toggle secondary Y-axis | `.YAxis()` accepts multiple `YAxis` instances |
| `xAxis.title` | X-axis label | `.XAxis()` configuration |
| `xAxis.scale` | X-axis scale: linear, log, time | `Scale` property on chart |
| `xAxis.rangeMin` / `rangeMax` | Manual axis bounds | Not supported |
| `xAxis.rangeMode` | Auto or manual axis range | Not supported |
| `xAxis.showGridLines` | Show grid lines on x-axis | `.CartesianGrid()` |
| `xAxis.showLine` | Show the axis line | `.XAxis()` configuration |
| `xAxis.showTickLabels` | Show tick labels | `.XAxis()` configuration |
| `xAxis.showZeroLine` | Show zero reference line | `ReferenceLine` at value 0 |
| `xAxis.sort` | Axis sorting: none, ascending, descending | Not supported (sort data before passing) |
| `xAxis.tickFormat` | Axis tick label format | `.XAxis()` configuration |
| `yAxis` | Primary Y-axis configuration (same sub-properties as xAxis) | `.YAxis()` configuration |
| `yAxis2` | Secondary Y-axis configuration | Additional `YAxis` instance in `.YAxis()` array |
| `toolbar.showAutoscale` | Show autoscale button | `.Toolbox()` |
| `toolbar.showBoxSelect` | Show box select button | Not supported |
| `toolbar.showLassoSelect` | Show lasso select button | Not supported |
| `toolbar.showPan` | Show pan button | `.Toolbox()` |
| `toolbar.showResetViews` | Show reset view button | `.Toolbox()` |
| `toolbar.showToImage` | Show download as PNG button | `.Toolbox()` (save as image) |
| `toolbar.showZoomIn` | Show zoom in button | `.Toolbox()` |
| `toolbar.showZoomOut` | Show zoom out button | `.Toolbox()` |
| `toolbar.showZoomSelect` | Show zoom select button | `.Toolbox()` |
| `margin` | Space outside chart (CSS values) | `.Height()` / `.Width()` sizing |
| `style` | Custom styling object | `.ColorScheme()` / `.Fill()` |
| `stackedBarTotalsDataLabelPosition` | Position of stacked bar total labels: center, inside, outside | Not supported |
| `scrollIntoView()` | Scroll component into visible area | Not supported |
| `setHidden()` | Toggle component visibility via JS | Not supported (server-driven visibility) |
| Events: Select | Triggered when a value is selected | Not supported |
| Events: Hover / Unhover | Triggered on hover/unhover | `.Tooltip()` for hover display |
| Events: Clear | Triggered when a value is cleared | Not supported |
| Events: Legend Click | Triggered on legend click | Not supported |
| `isHiddenOnMobile` | Hide on mobile layout | Not supported |
| `isHiddenOnDesktop` | Hide on desktop layout | Not supported |
| `maintainSpaceWhenHidden` | Keep layout space when hidden | Not supported |
| `ReferenceLines` | Not available as named property | `.ReferenceLines()` on all cartesian charts |
| `ReferenceAreas` | Not available as named property | `.ReferenceAreas()` on all cartesian charts |
| `ReferenceDots` | Not available as named property | `.ReferenceDots()` on all cartesian charts |
