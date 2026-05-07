# Sankey Chart

Renders data as links between nodes, useful for visualizing how data moves between different stages or categories (e.g., budget allocation, user flow, energy distribution).

## Retool

```toolscript
// Configure a Sankey Chart component via the Inspector:
// - Set chartType to "sankey"
// - Provide sankeyDatasource with source/target/value data
// - Customize node and link colors via colorArray / sankeyLinkColorArray
// - Add hover templates for tooltips

sankeyChart1.sankeyDatasource = {
  source: ["A", "A", "B", "B"],
  target: ["C", "D", "C", "D"],
  value:  [10, 20, 15, 25]
};

// Methods
sankeyChart1.setHidden(false);
sankeyChart1.scrollIntoView({ behavior: "smooth", block: "center" });

// Events: Clear, Hover, Select, Unhover, Legend Click, Legend Double Click
```

## Ivy

Ivy does not have a Sankey Chart widget. The supported chart types are:

```csharp
// Supported charts: LineChart, BarChart, AreaChart, PieChart
data.ToLineChart()
    .Dimension(x => x.Category)
    .Measure(x => x.Value)
    .Legend()
    .Tooltip();
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| chartType | The type of chart to display. Must be `"sankey"` for Sankey charts. | Not supported |
| sankeyDatasource | Data source for the chart (source, target, value arrays). | Not supported |
| sankeyDatasourceMode | Data mode: `"manual"` or `"source"`. | Not supported |
| sankeyAllowDuplicateNodesAtDifferentSteps | Whether to allow duplicate nodes at different steps. | Not supported |
| colorInputMode | Node color selection method: `colorArray`, `gradientColorArray`, or `colorArrayDropDown`. | Not supported |
| colorArray | Node color palette as array of hex values. | Not supported |
| gradientColorArray | Gradient color palette when `colorInputMode` is `gradientColorArray`. | Not supported |
| colorArrayDropDown | Manually selected color palette. | Not supported |
| sankeyLinkColorInputMode | Link color selection method: `colorArray`, `gradientColorArray`, or `colorArrayDropDown`. | Not supported |
| sankeyLinkColorArray | Link color scale as array of hex values. | Not supported |
| sankeyLinkGradientColorArray | Gradient link color scale. | Not supported |
| sankeyLinkColorArrayDropDown | Manually selected link color scale. | Not supported |
| sankeyNodeHoverTemplate | Customizable hover tooltip template for nodes. | Not supported |
| sankeyNodeHoverTemplateMode | Hover template editor mode: `"manual"` or `"source"`. | Not supported |
| sankeyLinkHoverTemplate | Customizable hover tooltip template for links. | Not supported |
| hidden | Whether the component is hidden from view. Default `false`. | Not supported |
| maintainSpaceWhenHidden | Whether to keep layout space when hidden. Default `false`. | Not supported |
| isHiddenOnDesktop | Whether to hide in the desktop layout. Default `false`. | Not supported |
| isHiddenOnMobile | Whether to hide in the mobile layout. Default `true`. | Not supported |
| showInEditor | Whether the component remains visible in editor when hidden. Default `false`. | Not supported |
| margin | External spacing around the component. Default `"4px 8px"`. | Not supported |
| style | Custom style options. | Not supported |
| paperBgColor | Paper (outer) background color. | Not supported |
| plotBgColor | Plot (inner) background color. | Not supported |
| toolbar | Toolbar configuration (autoscale, pan, zoom, lasso select, box select, download PNG, reset view). | Not supported |
| clearOnEmptyData | Whether to clear the chart when data is empty. Default `false`. | Not supported |
| events | Event handlers: Clear, Hover, Select, Unhover, Legend Click, Legend Double Click. | Not supported |
| id | Unique component identifier. Default `"sankeyChart1"`. | Not supported |
| scrollIntoView() | Scrolls the component into the visible area. | Not supported |
| setHidden() | Toggles component visibility programmatically. | Not supported |
