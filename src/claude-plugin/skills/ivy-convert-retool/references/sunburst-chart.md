# Sunburst Chart

Renders hierarchical data in a series of concentric rings. Useful for visualizing parent-child relationships and how parts make up a whole. Built on Plotly.

## Retool

```toolscript
// Configure a Sunburst Chart with labels, parents, and values
sunburstChart1.chartType = "sunburst";
sunburstChart1.labelData = ["Root", "Branch A", "Branch B", "Leaf 1", "Leaf 2", "Leaf 3"];
sunburstChart1.parentData = ["", "Root", "Root", "Branch A", "Branch A", "Branch B"];
sunburstChart1.valueData = [0, 0, 0, 10, 20, 30];
sunburstChart1.colorArray = ["#ff0000", "#00ff00", "#0000ff"];
sunburstChart1.legendPosition = "right";
sunburstChart1.hoverTemplate = "%{value}<br>%{percent}<extra></extra>";
```

## Ivy

Ivy does not have a dedicated Sunburst Chart widget. The closest equivalent is `PieChart` with drill-down support, which can represent hierarchical part-of-whole data interactively.

```csharp
// Pie chart with drill-down to simulate hierarchical exploration
data.ToPieChart(
    e => e.Category,
    e => e.Sum(f => f.Value),
    PieChartStyles.Dashboard
)
```

## Parameters

| Parameter                  | Documentation                                                                 | Ivy                                                      |
|----------------------------|-------------------------------------------------------------------------------|----------------------------------------------------------|
| `chartType`                | The type of chart to display (sunburst, pie, bar, etc.)                       | Not supported (no sunburst type)                         |
| `labelData`                | A list of labels for each node                                                | Key lambda in `ToPieChart(e => e.Month, ...)`            |
| `parentData`               | The parent to which a given node is associated                                | Not supported (flat structure; drill-down via `UseState`) |
| `valueData`                | The value for a specific node                                                 | Value lambda in `ToPieChart(..., e => e.Sum(...))`       |
| `datasource`               | The source data when using UI Form                                            | Data passed directly to `ToPieChart`                     |
| `colorArray`               | A list of colors to use                                                       | `ColorScheme`                                            |
| `colorInputMode`           | The mode for color input (array, palette, manual)                             | Not supported                                            |
| `gradientColorArray`       | A list of colors for gradient palette mode                                    | Not supported                                            |
| `legendPosition`           | The position of the legend (top, right, bottom, left, none)                   | `Legend` with `Align` enum                               |
| `hoverTemplate`            | Tooltip template for displaying info on hover (Plotly format)                 | `Tooltip`                                                |
| `textTemplate`             | Format for data labels in Plotly template format                              | `LabelList` with custom formatting                       |
| `textTemplatePosition`     | Orientation of data labels (radial, horizontal, vertical)                     | `LabelList` positioning options                          |
| `sunburstDataBranchValues` | Branch value calculation mode (remainder or total)                            | Not supported                                            |
| `sunburstDataLeafOpacity`  | Opacity of leaf nodes                                                         | Not supported                                            |
| `hidden`                   | Whether the component is hidden from view                                     | Not supported                                            |
| `isHiddenOnMobile`         | Whether to hide on mobile layout                                              | Not supported                                            |
| `isHiddenOnDesktop`        | Whether to hide on desktop layout                                             | Not supported                                            |
| `maintainSpaceWhenHidden`  | Whether to take up space when hidden                                          | Not supported                                            |
| `margin`                   | Amount of margin outside the component                                        | Not supported                                            |
| `toolbar`                  | Toolbar config (autoscale, zoom, pan, download, etc.)                         | `Toolbox`                                                |
| `selectedPoints`           | A list of selected points (read-only)                                         | Not supported                                            |
| `events`                   | Event handlers (Clear, Hover, Select, Legend Click, etc.)                     | Drill-down via `UseState`; `Animated()` for hover        |
| `clearOnEmptyData`         | Whether to clear the chart when data is empty                                 | Not supported                                            |
| `lineWidth`                | Width of the line (read-only)                                                 | Not supported                                            |
| `id`                       | Unique identifier (name)                                                      | Variable name in C#                                      |
