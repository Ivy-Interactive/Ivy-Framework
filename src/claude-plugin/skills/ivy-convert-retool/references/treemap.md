# Treemap

Displays hierarchical data as nested rectangles, useful for visualizing data with many categories and comparing the size and relationship of those categories. Built on Plotly.

## Retool

```toolscript
// Treemap with hierarchical data
treemap1.labelData = ["All", "Category A", "Category B", "Item 1", "Item 2", "Item 3"];
treemap1.parentData = ["", "All", "All", "Category A", "Category A", "Category B"];
treemap1.valueData = [0, 0, 0, 10, 20, 30];
treemap1.colorArray = ["#ff0000", "#00ff00", "#0000ff"];
treemap1.legendPosition = "bottom";
treemap1.hoverTemplate = "%{label}<br>%{value}<br>%{percentRoot:.1%}<extra></extra>";
```

## Ivy

Ivy does not have a dedicated Treemap widget. The closest equivalent is `PieChart` which can visualize hierarchical/proportional data with drill-down support via nested pies (sunburst-style).

```csharp
new PieChart(data)
    .Pies(
        new Pie()
            .DataKey("value")
            .NameKey("name")
            .Pie(p => p.OuterRadius(120))
            .LabelList(l => l.Position(LabelPositions.Inside).Fill(Colors.White))
    )
    .ColorScheme(ColorSchemes.Set2)
    .Tooltip(new Tooltip())
    .Legend(new Legend().Align(Aligns.Center).VerticalAlign(VerticalAligns.Bottom))
    .Height(400)
```

## Parameters

| Parameter          | Documentation                                                              | Ivy                                                       |
|--------------------|----------------------------------------------------------------------------|------------------------------------------------------------|
| labelData          | Array of strings for each node label                                       | Not supported (Treemap not available)                      |
| parentData         | Array of strings associating nodes to parents                              | Not supported (Treemap not available)                      |
| valueData          | Array of numbers representing node values                                  | `DataKey` on `Pie` (maps value field)                      |
| datasource         | Source data when using UI Form mode                                        | `Data` parameter on `PieChart` constructor                 |
| colorArray         | List of colors when `colorInputMode` is `colorArray`                       | `ColorScheme` (predefined palettes)                        |
| gradientColorArray | List of colors when `colorInputMode` is `gradientColorArray`               | Not supported                                              |
| colorInputMode     | Mode for color input (`colorArray`, `gradientColorArray`, `colorArrayDropDown`) | Not supported (only `ColorScheme` presets)             |
| legendPosition     | Position of the legend (`top`, `right`, `bottom`, `left`, `none`)          | `Legend().Align()` / `.VerticalAlign()`                    |
| hoverTemplate      | Tooltip format using Plotly template syntax                                | `Tooltip` (basic, no Plotly template syntax)               |
| textTemplate       | Data label format using Plotly template syntax                             | `LabelList` with `.NumberFormat()`                         |
| hidden             | Whether the component is hidden from view                                  | Not supported                                              |
| margin             | Spacing outside the component (`4px 8px` or `0`)                           | Not supported                                              |
| isHiddenOnDesktop  | Whether to hide on desktop layout                                          | Not supported                                              |
| isHiddenOnMobile   | Whether to hide on mobile layout                                           | Not supported                                              |
| maintainSpaceWhenHidden | Whether to reserve layout space when hidden                           | Not supported                                              |
| chartType          | Type of chart to display (bar, pie, treemap, etc.)                         | Not supported (each chart is a separate widget)            |
| clearOnEmptyData   | Whether to clear chart when data is empty                                  | Not supported                                              |
| selectedPoints     | List of selected points (read-only)                                        | Not supported                                              |
| toolbar.showAutoscale   | Show Autoscale button                                                 | `Toolbox` (general toolbox, no per-button control)         |
| toolbar.showBoxSelect   | Show Box select button                                                | Not supported                                              |
| toolbar.showLassoSelect | Show Lasso select button                                              | Not supported                                              |
| toolbar.showPan         | Show Pan button                                                       | Not supported                                              |
| toolbar.showResetViews  | Show Reset view button                                                | Not supported                                              |
| toolbar.showToImage     | Show Download as PNG button                                           | `Toolbox` (includes save-as-image)                         |
| toolbar.showZoomIn      | Show Zoom in button                                                   | Not supported                                              |
| toolbar.showZoomOut     | Show Zoom out button                                                  | Not supported                                              |
| toolbar.showZoomSelect  | Show Zoom select button                                               | Not supported                                              |
| scrollIntoView()   | Scrolls component into visible area                                        | Not supported                                              |
| setHidden()        | Toggles component visibility                                               | Not supported                                              |
| Event: Select      | A value is selected                                                        | Not supported                                              |
| Event: Hover       | An item is hovered over                                                    | Not supported                                              |
| Event: Clear       | A value is cleared                                                         | Not supported                                              |
| Event: Legend Click | A legend item is clicked                                                  | Not supported                                              |
