# Charts

Charts and graphs to represent data in web apps. Retool provides 15 chart types built on Plotly.js. Ivy provides 4 chart builders (LineChart, BarChart, AreaChart, PieChart) with a fluent C# API built on Recharts.

## Retool

```javascript
// Bar Chart - data is bound via the Inspector UI or JavaScript
barChart1.chartType = "bar";
barChart1.barMode = "group";        // stack | group | overlay | relative
barChart1.barOrientation = "vertical";
barChart1.legendPosition = "bottom";
barChart1.xAxis.sort = "ascending";
barChart1.xAxis.scale = "linear";

// Line Chart
lineChart1.chartType = "line";
lineChart1.legendPosition = "top";
lineChart1.rangeSlider = true;

// Pie Chart
pieChart1.chartType = "pie";
pieChart1.pieDataHole = 0.4;        // donut chart
pieChart1.colorInputMode = "colorArray";
pieChart1.colorArray = ["#ff0000", "#00ff00", "#0000ff"];
pieChart1.hoverTemplate = "%{value}<br>%{percent}<extra></extra>";
```

## Ivy

```csharp
// Bar Chart - fluent builder API on data collections
data.ToBarChart(
        e => e.Category,              // Dimension (X-axis)
        e => e.Sum(f => f.Value))     // Measure (Y-axis)
    .ColorScheme(ColorScheme.Default)
    .CartesianGrid()
    .Legend()
    .Tooltip()
    .Toolbox()
    .SortBy(SortOrder.Ascending);

// Line Chart
data.ToLineChart(
        e => e.Date,
        e => e.Sum(f => f.Price))
    .ColorScheme(ColorScheme.Rainbow)
    .CartesianGrid()
    .Legend()
    .Tooltip()
    .Height(400);

// Pie Chart
data.ToPieChart(
        e => e.Month,
        e => e.Sum(f => f.Mobile),
        PieChartStyles.Dashboard);

// Area Chart
data.ToAreaChart(
        e => e.Date,
        e => e.Sum(f => f.Value))
    .CartesianGrid()
    .Tooltip()
    .Legend();
```

## Chart Type Coverage

| Chart Type        | Retool            | Ivy               |
|-------------------|-------------------|--------------------|
| Bar Chart         | `bar`             | `ToBarChart()`     |
| Line Chart        | `line`            | `ToLineChart()`    |
| Pie Chart         | `pie`             | `ToPieChart()`     |
| Area Chart        | —                 | `ToAreaChart()`    |
| Stacked Bar Chart | `barMode: "stack"`| `StackOffset`      |
| Bubble Chart      | `bubble`          | Not supported      |
| Funnel Chart      | `funnel`          | Not supported      |
| Heat Map          | `heatmap`         | Not supported      |
| Mixed Chart       | `mixed`           | Not supported      |
| Scatter Chart     | `scatter`         | Not supported      |
| Sankey Chart      | `sankey`          | Not supported      |
| Sunburst Chart    | `sunburst`        | Not supported      |
| Treemap           | `treemap`         | Not supported      |
| Waterfall Chart   | `waterfall`       | Not supported      |
| Sparkline         | `line` (sparkline)| Not supported      |
| Plotly JSON       | `plotlyJson`      | Not supported      |

## Parameters

| Parameter              | Retool                                             | Ivy                                          |
|------------------------|----------------------------------------------------|----------------------------------------------|
| Data source            | Bind via Inspector or `{{ query.data }}`           | Pass collection to `ToBarChart()` etc.       |
| X-axis / Dimension     | Map column in Inspector > Content > Series         | `.Dimension(e => e.Field)`                   |
| Y-axis / Measure       | Map column in Inspector > Content > Series         | `.Measure(e => e.Sum(f => f.Field))`         |
| Chart title            | `title` (string)                                   | Not supported                                |
| Legend position         | `legendPosition` (top/right/bottom/left/none)      | `.Legend()` with `Align` enums (9 positions) |
| Tooltip                | `hoverTemplate` (Plotly template format)           | `.Tooltip()`                                 |
| Grid lines             | `xAxis.showGridLines` / `yAxis.showGridLines`      | `.CartesianGrid()`                           |
| Color scheme           | `colorArray` / `colorInputMode`                    | `.ColorScheme(ColorScheme.Default)`          |
| Sorting                | `xAxis.sort` (none/ascending/descending)           | `.SortBy(SortOrder.Ascending)`               |
| Axis scale             | `xAxis.scale` (linear/log/time)                    | `.Scale()` property                          |
| Axis range (min/max)   | `xAxis.rangeMin` / `xAxis.rangeMax`                | Not supported                                |
| Axis title             | `xAxis.title` / `yAxis.title`                      | `.XAxis().Label<XAxis>("text")` / `.YAxis().Label<YAxis>("text")` |
| Tick format            | `xAxis.tickFormat` / `yAxis.tickFormat`             | `.NumberFormat("$0,0")` on LabelList         |
| Bar gap                | `barGap` (0-1)                                     | `BarGap` (int)                               |
| Bar group gap          | `barGroupGap` (0-1)                                | `BarCategoryGap`                             |
| Bar orientation        | `barOrientation` (vertical/horizontal)             | `Layout` (Layouts enum)                      |
| Bar mode (stacking)    | `barMode` (stack/group/overlay/relative)            | `StackOffset` (StackOffsetTypes)             |
| Max bar size           | Not supported                                      | `MaxBarSize` (int)                           |
| Donut hole             | `pieDataHole` (0-1)                                | `.InnerRadius("40%")`                        |
| Pie labels             | `textTemplate` / `textTemplatePosition`             | `.LabelList()` with `Position`               |
| Data labels position   | `stackedBarTotalsDataLabelPosition` (center/inside/outside) | `.LabelList().Position(Positions.Outside)` |
| Range slider           | `rangeSlider` (boolean)                            | Not supported                                |
| Second Y-axis          | `showSecondYAxis` (boolean)                        | Multiple `YAxis[]`                           |
| Reference lines        | Not supported                                      | `ReferenceLines` / `ReferenceAreas` / `ReferenceDots` |
| Toolbar (zoom, pan)    | `toolbar` (showZoomIn, showPan, showBoxSelect, etc)| `.Toolbox()`                                 |
| Download as PNG        | `toolbar.showToImage`                              | Via `.Toolbox()`                             |
| Line style             | Not configurable (Plotly default)                  | `LineChartStyles.Default` / `.Dashboard` / `.Custom` |
| Line stroke width      | Not configurable                                   | `.StrokeWidth()`                             |
| Fill opacity (area)    | Not applicable                                     | `.FillOpacity(0.5)`                          |
| Animation              | Not configurable                                   | `.Animated(true)`                            |
| Corner radius (bars)   | Not supported                                      | `.Radius()`                                  |
| Visibility             | `hidden` / `setHidden()`                           | `Visible` (bool)                             |
| Height / Width         | Controlled by canvas layout                        | `.Height()` / `.Width()`                     |
| Margin                 | `margin` (CSS string)                              | Not supported                                |
| Mobile visibility      | `isHiddenOnMobile` / `isHiddenOnDesktop`           | Not supported                                |
| Selected points        | `selectedPoints` (read-only array)                 | Not supported                                |
| Clear on empty data    | `clearOnEmptyData` (boolean)                       | Not supported                                |
| Events                 | Select, Hover, Unhover, Clear, Legend Click/DblClick | Not supported                              |
| Custom style           | `style` (object)                                   | Not supported                                |
