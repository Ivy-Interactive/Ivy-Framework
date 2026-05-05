# Bar Chart

A chart component that renders data as vertical (or horizontal) rectangular bars, useful for comparing values across categories.

## Retool

```toolscript
barChart1.barMode = "group";
barChart1.barOrientation = "vertical";
barChart1.barGap = 0.4;
barChart1.barGroupGap = 0;
barChart1.title = "Quarterly Sales";
barChart1.legendPosition = "top";
barChart1.xAxis = { title: "Month", showGridLines: true, sort: "none" };
barChart1.yAxis = { title: "Revenue", scale: "linear", rangeMode: "auto" };
barChart1.toolbar = { showToImage: true, showZoomIn: true, showResetViews: true };
barChart1.rangeSlider = true;
```

## Ivy

```csharp
var data = new[]
{
    new { Month = "Jan", Desktop = 186, Mobile = 100 },
    new { Month = "Feb", Desktop = 305, Mobile = 200 },
    new { Month = "Mar", Desktop = 237, Mobile = 300 },
};

return Layout.Vertical()
    | data.ToBarChart()
            .Dimension("Month", e => e.Month)
            .Measure("Desktop", e => e.Sum(f => f.Desktop))
            .Measure("Mobile", e => e.Sum(f => f.Mobile))
            .CartesianGrid(new CartesianGrid().Horizontal())
            .XAxis(new XAxis("Month").TickLine(false).AxisLine(false))
            .Legend()
            .Tooltip()
            .Toolbox();
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Bar gap | `barGap` (0-1, default `0.4`) — gap between bars relative to bar size | `BarGap` (int) |
| Bar group gap | `barGroupGap` (0-1, default `0`) — gap between bars in a group | `BarCategoryGap` (object) |
| Bar mode | `barMode` (`stack`, `group`, `overlay`, `relative`) | `StackOffset` (StackOffsetTypes) + `ReverseStackOrder` (bool) |
| Orientation | `barOrientation` (`vertical`, `horizontal`) | `Layout` (Layouts) |
| Chart type | `chartType` — switches between bar, line, pie, scatter, etc. | Not supported (separate widget types: `BarChart`, `LineChart`, `PieChart`, etc.) |
| Title | `title` — chart title text | Not supported (use a separate text widget or axis titles) |
| Legend position | `legendPosition` (`top`, `right`, `bottom`, `left`, `none`) | `Legend` (Legend object via `.Legend()`) |
| X-axis config | `xAxis` — object with `title`, `scale`, `sort`, `tickFormat`, `showGridLines`, `showLine`, `showTickLabels`, `showZeroLine`, `rangeMin`, `rangeMax`, `rangeMode` | `XAxis` (XAxis[] via `.XAxis()`) with `TickLine`, `AxisLine`, and title |
| Y-axis config | `yAxis` — same sub-properties as xAxis | `YAxis` (YAxis[] via `.YAxis()`) |
| Toolbar | `toolbar` — object with booleans for zoom, pan, box select, lasso select, download PNG, reset views, autoscale | `Toolbox` (Toolbox via `.Toolbox()`) |
| Tooltip | Built-in hover tooltip on data points | `Tooltip` (Tooltip via `.Tooltip()`) |
| Grid lines | `xAxis.showGridLines` / `yAxis.showGridLines` | `CartesianGrid` (CartesianGrid via `.CartesianGrid()`) |
| Color scheme | `style` — custom style object | `ColorScheme` (ColorScheme) or per-bar `Fill(Colors.X)` |
| Max bar size | Not supported | `MaxBarSize` (int?) |
| Reference lines | Not supported | `ReferenceLines` (ReferenceLine[]), `ReferenceAreas`, `ReferenceDots` |
| Hidden | `hidden` (bool) + `setHidden()` method | `Visible` (bool) |
| Range slider | `rangeSlider` (bool) — shows a range slider below the chart | Not supported |
| Selected points | `selectedPoints` — read-only array of selected data points | Not supported |
| Clear on empty | `clearOnEmptyData` (bool) — clears chart when data is empty | Not supported |
| Events | `Select`, `Hover`, `Unhover`, `Clear`, `Legend Click`, `Legend Double Click` | Not supported (Ivy uses state-driven reactivity instead of event handlers) |
| Mobile/Desktop visibility | `isHiddenOnMobile` / `isHiddenOnDesktop` | Not supported |
| Scroll into view | `scrollIntoView()` method | Not supported |
| Margin | `margin` (`4px 8px` or `0`) | Not supported |
