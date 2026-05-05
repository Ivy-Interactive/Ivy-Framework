# Heat Map

Renders data on an x-axis and y-axis with a value for each point on the map. Useful for visualizing intensity and identifying patterns or correlations.

## Retool

```js
// Heat Map configured via component properties
heatMap1.chartType = "heatmap";
heatMap1.data = {{query1.data}};
heatMap1.title = "User Activity";
heatMap1.colorInputMode = "gradientColorArray";
heatMap1.gradientColorArray = ["#f7fbff", "#08306b"];
heatMap1.heatmapShowScale = true;
heatMap1.legendPosition = "right";
heatMap1.toolbar = {
  showZoomIn: true,
  showZoomOut: true,
  showResetViews: true,
  showToImage: true
};

// Methods
heatMap1.setHidden(false);
heatMap1.scrollIntoView({ behavior: "smooth", block: "center" });

// Read selected points
const selected = heatMap1.selectedPoints;
```

## Ivy

```csharp
// Ivy does not have a Heat Map widget.
// The supported chart types are: LineChart, BarChart, AreaChart, and PieChart.
//
// Example of the closest alternative — a BarChart with color scheme:
data.ToBarChart()
    .Dimension(x => x.Category)
    .Measure(x => x.Value)
    .ColorScheme(ColorScheme.Rainbow)
    .Tooltip()
    .Legend()
    .Toolbox()
    .CartesianGrid()
    .XAxis(x => x.Label("Category"))
    .YAxis(y => y.Label("Value"));
```

## Parameters

| Parameter | Documentation | Ivy |
|---|---|---|
| `chartType` | The type of chart to display. Set to `"heatmap"` for heat maps. | Not supported (Ivy has Line, Bar, Area, Pie only) |
| `data` | The dataset to render on the heat map. | `object data` constructor parameter on chart widgets |
| `datasource` | Source data when using UI Form mode. | Not supported |
| `title` | Display title text. | Not supported (no built-in title property on charts) |
| `colorInputMode` | Color mode: `colorArray`, `gradientColorArray`, or `colorArrayDropDown`. | `.ColorScheme()` (limited to `Default` and `Rainbow`) |
| `colorArray` | List of colors when mode is `colorArray`. | Not supported |
| `gradientColorArray` | List of gradient colors when mode is `gradientColorArray`. | Not supported |
| `colorArrayDropDown` | List of colors when mode is `colorArrayDropDown`. | Not supported |
| `heatmapShowScale` | Whether to display the color scale legend. | Not supported |
| `legendPosition` | Legend position: `top`, `right`, `bottom`, `left`, or `none`. | `.Legend()` with `.Layout()` and `.Align()` |
| `hidden` | Whether the component is hidden from view. | Not supported |
| `margin` | External spacing (`"4px 8px"` or `"0"`). | Not supported |
| `toolbar.showAutoscale` | Show Autoscale button in toolbar. | Not supported |
| `toolbar.showBoxSelect` | Show Box Select button in toolbar. | Not supported |
| `toolbar.showLassoSelect` | Show Lasso Select button in toolbar. | Not supported |
| `toolbar.showPan` | Show Pan button in toolbar. | Not supported |
| `toolbar.showResetViews` | Show Reset View button in toolbar. | Not supported |
| `toolbar.showToImage` | Show Download as PNG button in toolbar. | `.Toolbox()` (provides built-in chart tools) |
| `toolbar.showZoomIn` | Show Zoom In button in toolbar. | `.Toolbox()` |
| `toolbar.showZoomOut` | Show Zoom Out button in toolbar. | `.Toolbox()` |
| `toolbar.showZoomSelect` | Show Zoom Select button in toolbar. | Not supported |
| `rangeSlider` | Whether to display a range slider. | Not supported |
| `selectedPoints` | Read-only list of selected points. | Not supported |
| `clearOnEmptyData` | Clear chart when data is empty. | Not supported |
| `isHiddenOnDesktop` | Hide on desktop layout. | Not supported |
| `isHiddenOnMobile` | Hide on mobile layout. | Not supported |
| `maintainSpaceWhenHidden` | Keep space when hidden. | Not supported |
| `scrollIntoView()` | Scroll component into visible area. | Not supported |
| `setHidden()` | Toggle component visibility. | Not supported |
| **Event: Clear** | Triggered when a value is cleared. | Not supported |
| **Event: Hover** | Triggered on item hover. | `.Tooltip()` (shows data on hover) |
| **Event: Select** | Triggered on value selection. | Not supported |
| **Event: Unhover** | Triggered when hover exits. | Not supported |
| **Event: Legend Click** | Triggered on legend item click. | Not supported |
| **Event: Legend Double Click** | Triggered on legend item double-click. | Not supported |
