# Line Chart

Renders data as lines on x/y axes, useful for visualizing trends over time. Supports multiple data series, interactive toolbars, legends, tooltips, and customizable axis configuration.

## Retool

```toolscript
// Line Chart component configured via Inspector
// Data: query result with columns for x-axis and series values
lineChart1.chartType = "line";
lineChart1.title = "Sales Over Time";
lineChart1.xAxis = { title: "Month", scale: "time", showGridLines: true };
lineChart1.yAxis = { title: "Revenue", scale: "linear", rangeMode: "auto" };
lineChart1.legendPosition = "bottom";
lineChart1.toolbar = { showZoomIn: true, showZoomOut: true, showResetViews: true, showToImage: true };
```

## Ivy

```csharp
public class SalesLineChart : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "January", Desktop = 186, Mobile = 100 },
            new { Month = "February", Desktop = 305, Mobile = 200 },
            new { Month = "March", Desktop = 237, Mobile = 300 },
            new { Month = "April", Desktop = 186, Mobile = 100 },
            new { Month = "May", Desktop = 325, Mobile = 200 }
        };
        return Layout.Vertical()
                 | data.ToLineChart(style: LineChartStyles.Default)
                        .Dimension("Month", e => e.Month)
                        .Measure("Desktop", e => e.Sum(f => f.Desktop))
                        .Measure("Mobile", e => e.Sum(f => f.Mobile))
                        .Legend()
                        .Tooltip()
                        .CartesianGrid()
                        .XAxis()
                        .YAxis()
                        .Toolbox();
    }
}
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Chart type | `chartType` — supports line, bar, bubble, funnel, heatmap, pie, scatter, etc. | Separate widget per type: `ToLineChart()`, `ToBarChart()`, `ToPieChart()`, `ToAreaChart()` |
| Data source | Component data binding via Inspector | Passed as constructor arg or via `.ToLineChart()` extension on collections |
| Title | `title` — string | Not a direct property; use `Text.P()` above the chart |
| X-axis config | `xAxis` object — `title`, `scale` (linear/log/time), `rangeMin`, `rangeMax`, `rangeMode`, `sort`, `tickFormat`, `showGridLines`, `showLine`, `showTickLabels`, `showZeroLine` | `.XAxis()` — configurable axis layout |
| Y-axis config | `yAxis` object — same sub-properties as xAxis | `.YAxis()` — configurable axis layout |
| Second Y-axis | `showSecondYAxis` — boolean | Not supported |
| X-axis scale | `xAxis.scale` — `linear`, `log`, `time` | `.Scale` property on chart |
| Y-axis scale | `yAxis.scale` — `linear`, `log`, `time` | `.Scale` property on chart |
| Axis range | `xAxis.rangeMode` (`auto`/`manual`), `rangeMin`, `rangeMax` | Not supported |
| Axis sort | `xAxis.sort` / `yAxis.sort` — `none`, `ascending`, `descending` | `SortOrder.None`, `SortOrder.Ascending`, `SortOrder.Descending` via `.SortBy()` |
| Grid lines | `xAxis.showGridLines` / `yAxis.showGridLines` — boolean | `.CartesianGrid()` |
| Tick labels | `xAxis.showTickLabels` / `yAxis.showTickLabels` — boolean | Not supported |
| Tick format | `xAxis.tickFormat` / `yAxis.tickFormat` — string | Not supported |
| Zero line | `xAxis.showZeroLine` / `yAxis.showZeroLine` — boolean | Not supported |
| Legend | `legendPosition` — `top`, `right`, `bottom`, `left`, `none` | `.Legend()` |
| Tooltip | Built-in hover tooltips | `.Tooltip()` |
| Toolbar | `toolbar` object — `showZoomIn`, `showZoomOut`, `showPan`, `showBoxSelect`, `showLassoSelect`, `showResetViews`, `showAutoscale`, `showToImage`, `showZoomSelect` | `.Toolbox()` |
| Range slider | `rangeSlider` — boolean | Not supported |
| Hidden | `hidden` — boolean | `.Visible` property |
| Hidden on desktop | `isHiddenOnDesktop` — boolean | Not supported |
| Hidden on mobile | `isHiddenOnMobile` — boolean | Not supported |
| Maintain space when hidden | `maintainSpaceWhenHidden` — boolean | Not supported |
| Show in editor | `showInEditor` — boolean | Not supported |
| Margin | `margin` — `"4px 8px"` or `"0"` | Not supported |
| Style | `style` — custom CSS object | `LineChartStyles.Default`, `LineChartStyles.Dashboard`, `LineChartStyles.Custom` |
| Line width | Via Plotly trace config | `.StrokeWidth()` on individual `Line` objects |
| Color scheme | Via Plotly trace colors | `.ColorScheme()` — `Default`, `Rainbow` |
| Dimensions (data) | Column mapping via Inspector | `.Dimension("label", selector)` — defines X-axis grouping |
| Measures (data) | Column mapping via Inspector | `.Measure("label", aggregation)` — defines Y-axis values with aggregation |
| Width/Height | Component resize on canvas | `.Width()` / `.Height()` |
| Reference lines | Not built-in (use Plotly JSON shapes) | `.ReferenceLines` — `ReferenceLine[]` |
| Reference areas | Not built-in (use Plotly JSON shapes) | `.ReferenceAreas` — `ReferenceArea[]` |
| Reference dots | Not built-in | `.ReferenceDots` — `ReferenceDot[]` |
| Bar mode | `barMode` — `stack`, `group`, `overlay`, `relative` | Not applicable (line chart specific) |
| Bar gap | `barGap` — float 0-1 | Not applicable (line chart specific) |
| Bar orientation | `barOrientation` — `vertical`, `horizontal` | Not applicable (line chart specific) |
| Selected points | `selectedPoints` — read-only array | Not supported |
| Clear on empty data | `clearOnEmptyData` — boolean | Not supported |
| Event handlers | `events` — array of handler objects (Select, Hover, Unhover, Clear, Legend Click, Legend Double Click) | Not supported (use Ivy state management) |
| Methods | `scrollIntoView()`, `setHidden()` | Not supported |
| ID | `id` — unique component name | Not applicable (C# variable reference) |
