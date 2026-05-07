# Pie Chart

Renders data in a circle with percentage-based slices, useful for visualizing how different parts make up a whole. Supports donut charts, custom labels, tooltips, and legend positioning.

## Retool

```toolscript
// Basic Pie Chart configuration
pieChart1.chartType = "pie"
pieChart1.labelData = ["January", "February", "March"]
pieChart1.valueData = [186, 305, 237]
pieChart1.colorArray = ["#ff0000", "#00ff00", "#0000ff"]
pieChart1.legendPosition = "bottom"
pieChart1.hoverTemplate = "%{label}<br>%{value}<br>%{percent}<extra></extra>"

// Donut chart (hole depth 0-1)
pieChart1.pieDataHole = 0.4

// Event handlers
pieChart1.events = [{ event: "Select", type: "script", method: "handleSelect" }]
```

## Ivy

```csharp
public class PieChartDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "January", Desktop = 186, Mobile = 100 },
            new { Month = "February", Desktop = 305, Mobile = 200 },
            new { Month = "March", Desktop = 237, Mobile = 300 },
        };

        return Layout.Vertical()
            | Text.P("Mobile sales over Q1").Large()
            | data.ToPieChart(
                e => e.Month,
                e => e.Sum(f => f.Mobile),
                PieChartStyles.Dashboard)
              .Toolbox(new Toolbox())
            | Text.P("Desktop sales over Q1").Large()
            | data.ToPieChart(
                e => e.Month,
                e => e.Sum(f => f.Desktop),
                PieChartStyles.Donut);
    }
}
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Chart type | `chartType` — string (`"pie"`, `"bar"`, `"line"`, etc.) | `PieChartStyles` enum (`Dashboard`, `Donut`, etc.) |
| Data source | `valueData` (number[]) + `labelData` (string[]) | `Data` object or `ToPieChart()` with lambda projections for key/value |
| Colors | `colorArray` / `colorInputMode` (`"colorArray"`, `"gradientColorArray"`, `"colorArrayDropDown"`) | `.ColorScheme(ColorScheme.Default)` |
| Legend position | `legendPosition` — `"top"`, `"right"`, `"bottom"`, `"left"`, `"none"` | `.Legend(new Legend())` with `Align` enum (nine positions) and `.IconType()` |
| Tooltip / Hover | `hoverTemplate` — Plotly template string (e.g. `%{value}<br>%{percent}`) | `.Tooltip(new Tooltip().Animated(true))` |
| Donut hole | `pieDataHole` — number 0–1 | `.Pie(new Pie(...).InnerRadius("40%").OuterRadius("90%"))` |
| Data labels | `textTemplate` — Plotly format string; `textTemplatePosition` — `"radial"`, `"horizontal"`, `"vertical"` | `.LabelList(new LabelList(...).Position(Positions.Outside).NumberFormat("$0,0"))` — supports multiple independent label layers |
| Visibility | `hidden` — boolean; `setHidden(bool)` method | `Visible` — bool |
| Dimensions | Not directly configurable (layout-driven) | `Height` / `Width` — `Size` |
| Margin / Spacing | `margin` — `"4px 8px"` or `"0"` | Layout-driven via `Layout.Vertical()` / `Layout.Horizontal()` |
| Toolbar | `toolbar` object — toggle individual buttons (`showZoomIn`, `showToImage`, `showPan`, etc.) | `.Toolbox(new Toolbox())` |
| Total display | Not supported | `.Total(value, "Label")` — displays aggregate value in donut center |
| Drill-down | Event handlers (`Select` event triggers scripts) | `UseState` + `ToSelectInput` for interactive drill-down navigation |
| Animation | Not supported (Plotly default transitions) | `.Animated(true)` on `Pie` and `Tooltip` |
| Events | `Clear`, `Hover`, `Legend Click`, `Legend Double Click`, `Select`, `Unhover` | Not supported (interaction via state management) |
| Scale | Not supported | `Scale` property |
| Scroll into view | `scrollIntoView(options)` method | Not supported |
| Desktop/Mobile visibility | `isHiddenOnDesktop` / `isHiddenOnMobile` | Not supported |
| Maintain space when hidden | `maintainSpaceWhenHidden` | Not supported |
| Line color/width | `lineColor` / `lineWidth` — border styling on slices | `.Fill(Colors.Blue)` on `LabelList` (label styling, not slice borders) |
| Clear on empty data | `clearOnEmptyData` — boolean | Not supported |
