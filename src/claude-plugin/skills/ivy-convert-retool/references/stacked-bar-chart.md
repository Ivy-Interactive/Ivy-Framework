# Stacked Bar Chart

Renders data in vertical or horizontal bars, with stacked segments representing different groups or subcategories. Useful for comparing categorical data across multiple groups.

## Retool

```toolscript
// Stacked Bar Chart is configured via the property panel.
// barMode defaults to "stack" for stacked bars.
// Methods:
stackedBarChart.scrollIntoView({ behavior: 'auto', block: 'nearest' });
stackedBarChart.setHidden(true);
```

## Ivy

```csharp
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
        .Toolbox();
```

## Parameters

| Parameter         | Documentation                                                                       | Ivy                                          |
|-------------------|-------------------------------------------------------------------------------------|----------------------------------------------|
| barGap            | Gap between bars within a category (0–1, default 0.4)                               | `BarGap` (int)                               |
| barGroupGap       | Gap between bar groups (0–1, default 0)                                             | `BarCategoryGap`                             |
| barMode           | Display mode: stack, group, overlay, relative (default "stack")                     | Stacked by default; `StackOffset` for modes  |
| barOrientation    | Bar direction: vertical or horizontal                                               | `Layout` (Layouts enum)                      |
| chartType         | Chart type identifier (always "bar")                                                | Not applicable (type is implicit)            |
| clearOnEmptyData  | Clear chart when data is empty (default false)                                      | Not supported                                |
| hidden            | Control component visibility (default false)                                        | `Visible` (bool)                             |
| legendPosition    | Legend placement: top, right, bottom, left, none                                    | `Legend` (Legend object)                      |
| title             | Chart title text                                                                    | Not supported                                |
| margin            | Outer spacing (default "4px 8px")                                                   | Not supported                                |
| rangeSlider       | Enable range slider for zooming                                                     | `Toolbox` (includes zoom controls)           |
| xAxisScaleType    | X-axis scale: linear, logarithmic, time                                             | `XAxis` (XAxis[] with Scale)                 |
| yAxisScaleType    | Y-axis scale: linear, logarithmic, time                                             | `YAxis` (YAxis[] with Scale)                 |
| xAxisGridLines    | Toggle x-axis grid lines                                                            | `CartesianGrid`                              |
| yAxisGridLines    | Toggle y-axis grid lines                                                            | `CartesianGrid`                              |
| xAxisRange        | Custom x-axis min/max range (auto or manual)                                        | `XAxis` configuration                        |
| yAxisRange        | Custom y-axis min/max range (auto or manual)                                        | `YAxis` configuration                        |
| maxBarSize        | Maximum bar width                                                                   | `MaxBarSize` (int?)                          |
| colorScheme       | Color palette for bar series                                                        | `ColorScheme` / `.Fill(Colors)` per bar      |
| tooltip           | Enable tooltip on hover                                                             | `.Tooltip()`                                 |
| referenceLines    | Overlay reference lines on chart                                                    | `ReferenceLines` (ReferenceLine[])           |
| reverseStackOrder | Reverse the order of stacked segments                                               | `ReverseStackOrder` (bool)                   |
| scrollIntoView()  | Scroll component into view                                                          | Not supported                                |
| setHidden()       | Programmatically show/hide                                                          | `Visible` (bool)                             |
