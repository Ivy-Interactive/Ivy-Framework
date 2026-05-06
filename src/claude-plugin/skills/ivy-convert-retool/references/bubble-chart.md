# Bubble Chart

A data visualization component that plots data points using x-axis and y-axis positions, with bubble size representing a third data dimension. Useful for visually identifying relationships between three numeric variables.

## Retool

```toolscript
bubbleChart1.setChartType("bubble");
bubbleChart1.setSeries([
  {
    label: "Products",
    dataX: {{ productsQuery.data.price }},
    dataY: {{ productsQuery.data.revenue }},
    dataSize: {{ productsQuery.data.marketShare }},
  }
]);
bubbleChart1.setXAxis({ title: "Price", scale: "linear" });
bubbleChart1.setYAxis({ title: "Revenue", scale: "linear" });
bubbleChart1.setTitle("Product Performance");
bubbleChart1.setLegendPosition("right");
```

## Ivy

Not directly supported. Ivy provides LineChart, BarChart, AreaChart, and PieChart. A bubble chart could be implemented via an [External Widget](https://docs.ivy.app/widgets/advanced/external-widgets) using a React charting library (e.g. Recharts, Plotly, Chart.js).

```csharp
// No built-in bubble chart. Closest pattern for reference (BarChart):
data.ToBarChart()
    .Dimension("Category", e => e.Category)
    .Measure("Value", e => e.Sum(f => f.Value))
    .Tooltip()
    .Legend()
    .Toolbox();
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| chartType | Type of chart to render (`"bubble"`, `"bar"`, `"line"`, etc.) | Not supported (no bubble type) |
| series | Chart series data with x, y, and size values | `.Measure()` / `.Dimension()` (no size dimension) |
| title | Display title text | Not supported on chart directly |
| xAxis | X-axis configuration (scale, range, title, grid lines, tick labels) | `.XAxis()` with label configuration |
| yAxis | Y-axis configuration (scale, range, title, grid lines, tick labels) | `.YAxis()` with label configuration |
| xAxis.scale | Axis scale: `"linear"`, `"log"`, or `"time"` | Not supported |
| xAxis.rangeMode | `"auto"` or `"manual"` axis range | Not supported |
| xAxis.sort | `"none"`, `"ascending"`, `"descending"` | `.SortBy(SortOrder.Ascending)` etc. |
| xAxis.showGridLines | Show grid lines on axis | `.CartesianGrid()` |
| xAxis.showTickLabels | Show tick labels on axis | Not supported |
| xAxis.tickFormat | Custom label formatting | Not supported |
| legendPosition | Legend placement: `top`, `right`, `bottom`, `left`, `none` | `.Legend()` with layout options |
| selectedPoints | Read-only array of selected data points | Not supported |
| rangeSlider | Display a range slider control | Not supported |
| clearOnEmptyData | Clear chart when data is empty (default: `false`) | Not supported |
| hidden | Hide the component from view | Not supported |
| dataLabels | Customizable position and template for data labels | Not supported |
| hoverTooltip | Tooltip format on hover | `.Tooltip()` |
| toolbar | Autoscale, box/lasso select, pan, zoom, reset, download as PNG | `.Toolbox()` |
| colorScheme | Color configuration for series | `.ColorScheme(ColorScheme.Rainbow)` |
| scrollIntoView() | Method to scroll component into view | Not supported |
| setHidden() | Method to toggle visibility | Not supported |
| Events: Select, Hover, Unhover, Clear, Legend Click | Interactive event handlers | Not supported |
