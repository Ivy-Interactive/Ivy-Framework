# Plotly JSON Chart

Displays custom charts using raw Plotly JSON structured data. Provides full control over the data and layout of the chart, useful when you need customization beyond standard chart components.

## Retool

```toolscript
plotlyJsonChart1.plotlyDataJson = [
  {
    "x": ["Jan", "Feb", "Mar", "Apr"],
    "y": [10, 15, 13, 17],
    "type": "scatter",
    "mode": "lines+markers",
    "name": "Desktop"
  },
  {
    "x": ["Jan", "Feb", "Mar", "Apr"],
    "y": [16, 5, 11, 9],
    "type": "bar",
    "name": "Mobile"
  }
];

plotlyJsonChart1.plotlyLayoutJson = {
  "title": "Monthly Traffic",
  "xaxis": { "title": "Month" },
  "yaxis": { "title": "Visitors" }
};
```

## Ivy

Ivy does not have a raw Plotly JSON chart widget. The closest equivalent is using the typed chart builders (LineChart, BarChart, AreaChart, PieChart) with the fluent API.

```csharp
var data = new[]
{
    new { Month = "Jan", Desktop = 10, Mobile = 16 },
    new { Month = "Feb", Desktop = 15, Mobile = 5 },
    new { Month = "Mar", Desktop = 13, Mobile = 11 },
    new { Month = "Apr", Desktop = 17, Mobile = 9 },
};

// Line chart equivalent
data.ToLineChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .Measure("Mobile", e => e.Sum(f => f.Mobile))
    .XAxis(new XAxis("Month"))
    .Tooltip()
    .Legend()
    .Toolbox();

// Bar chart equivalent
data.ToBarChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .Measure("Mobile", e => e.Sum(f => f.Mobile))
    .XAxis(new XAxis("Month"))
    .Tooltip()
    .Legend();
```

## Parameters

| Parameter              | Documentation                                                    | Ivy                                                                       |
|------------------------|------------------------------------------------------------------|---------------------------------------------------------------------------|
| plotlyDataJson         | Chart data in Plotly JSON format (array of trace objects)        | Not supported (use typed builders: `.ToLineChart()`, `.ToBarChart()` etc.) |
| plotlyLayoutJson       | Layout configuration in Plotly JSON format                       | Not supported (layout configured via fluent methods)                      |
| chartType              | Chart display type, set to `plotlyJson`                          | Separate classes: `LineChart`, `BarChart`, `AreaChart`, `PieChart`         |
| title                  | Title text displayed above the chart                             | Not documented as a direct property                                       |
| selectedPoints         | Read-only array of user-selected data points                     | Not supported                                                             |
| clearOnEmptyData       | Clear chart when data is empty (default: false)                  | Not supported                                                             |
| hidden                 | Controls component visibility (default: false)                   | `Visible` property on chart widgets                                       |
| margin                 | Outer margin rendering ("4px 8px" or "0")                        | Not supported                                                             |
| id                     | Unique component identifier                                      | Not supported (widgets identified by variable binding)                    |
| isHiddenOnDesktop      | Hide on desktop layouts                                          | Not supported                                                             |
| isHiddenOnMobile       | Hide on mobile layouts                                           | Not supported                                                             |
| maintainSpaceWhenHidden| Reserve layout space when hidden                                 | Not supported                                                             |
| toolbar.showAutoscale  | Show autoscale button in toolbar                                 | `.Toolbox()` enables a toolbar but individual buttons are not configurable |
| toolbar.showBoxSelect  | Show box select tool                                             | Not supported                                                             |
| toolbar.showLassoSelect| Show lasso select tool                                           | Not supported                                                             |
| toolbar.showPan        | Show pan tool                                                    | Not supported                                                             |
| toolbar.showResetViews | Show reset views button                                          | Not supported                                                             |
| toolbar.showToImage    | Show save-as-image button                                        | `.Toolbox()` includes save-as-image                                       |
| toolbar.showZoomIn     | Show zoom in button                                              | `.Toolbox()` includes zoom                                                |
| toolbar.showZoomOut    | Show zoom out button                                             | `.Toolbox()` includes zoom                                                |
| toolbar.showZoomSelect | Show zoom select tool                                            | Not supported                                                             |
| scrollIntoView()       | Scrolls component into view                                      | Not supported                                                             |
| setHidden()            | Toggle component visibility                                      | `Visible` property                                                        |
| Event: Select          | Fires when data points are selected                              | Not supported                                                             |
| Event: Hover           | Fires when hovering over data points                             | `.Tooltip()` shows hover info but no event handler                        |
| Event: Legend Click     | Fires when legend item is clicked                               | Not supported                                                             |
