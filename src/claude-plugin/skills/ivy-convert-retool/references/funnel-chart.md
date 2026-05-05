# Funnel Chart

Renders data in a funnel shape with sections of decreasing size. Useful for visualizing data flowing through sequential stages of a process (e.g., sales pipeline, conversion funnel).

## Retool

```toolscript
// Funnel Chart component configured via Inspector
// Data source: array of objects with stage labels and values
// Chart type set to "funnel"

funnelChart1.chartType = "funnel";
funnelChart1.datasource = [
  { stage: "Visitors",     count: 5000 },
  { stage: "Leads",        count: 3200 },
  { stage: "Qualified",    count: 1800 },
  { stage: "Proposals",    count: 900  },
  { stage: "Closed Deals", count: 400  }
];

// Tooltip template
funnelChart1.hoverTemplate = "%{value}<br>%{percent}<extra></extra>";

// Event handlers
funnelChart1.events = [{ event: "Select", type: "script", method: "handleSelect" }];
```

## Ivy

Ivy does not have a dedicated Funnel Chart widget. The closest equivalent is `BarChart`, which can visualize stage-based data with horizontal bars of decreasing size.

```csharp
public class FunnelAlt : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Stage = "Visitors",     Count = 5000 },
            new { Stage = "Leads",        Count = 3200 },
            new { Stage = "Qualified",    Count = 1800 },
            new { Stage = "Proposals",    Count = 900  },
            new { Stage = "Closed Deals", Count = 400  },
        };

        return data.ToBarChart()
            .Dimension("Stage", e => e.Stage)
            .Measure("Count", e => e.Sum(f => f.Count))
            .Layout(Layouts.Vertical)
            .Tooltip()
            .Toolbox();
    }
}
```

## Parameters

| Parameter            | Documentation                                                      | Ivy                                                |
|----------------------|--------------------------------------------------------------------|----------------------------------------------------|
| `chartType`          | Type of chart (`funnel`, `bar`, `pie`, etc.)                       | Not supported (no unified chart-type switcher)     |
| `datasource`         | Source data array for the chart                                    | Constructor `data` parameter on `BarChart`          |
| `series`             | Chart series configuration (xData, yData, aggregation)             | `.Dimension()` / `.Measure()` on `ToBarChart`       |
| `aggregationType`    | Aggregation: sum, average, count, min, max, median, etc.           | Lambda in `.Measure()` (e.g. `e.Sum(...)`)          |
| `groupBy`            | Column to group by                                                 | `.Dimension()` key selector                         |
| `colorArray`         | Custom color list                                                  | `.Fill(Colors.X)` per `Bar`                         |
| `colorInputMode`     | Color mode: array, gradient palette, or manual                     | `.ColorScheme(ColorScheme.X)`                       |
| `gradientColorArray` | Gradient palette colors                                            | Not supported                                      |
| `legendPosition`     | Position of legend: top, right, bottom, left, none                 | `.Legend(new Legend().Align(...))`                   |
| `hoverTemplate`      | Tooltip template (Plotly format with `%{value}`, `%{percent}`)     | `.Tooltip(new Tooltip())`                           |
| `dataLabelTemplate`  | Data label template                                                | `.LabelList(new LabelList(...))`                    |
| `dataLabelPosition`  | Position of data labels: top or left                               | `.LabelList(...).Position(Positions.X)`             |
| `hidden`             | Whether the component is hidden                                    | `.Visible` property                                 |
| `isHiddenOnMobile`   | Hide on mobile layout                                              | Not supported                                      |
| `isHiddenOnDesktop`  | Hide on desktop layout                                             | Not supported                                      |
| `maintainSpaceWhenHidden` | Keep layout space when hidden                                 | Not supported                                      |
| `margin`             | Outer margin (`4px 8px` or `0`)                                    | Not supported (handled by layout)                  |
| `toolbar`            | Toolbar buttons (zoom, pan, reset, download, etc.)                 | `.Toolbox(new Toolbox())`                           |
| `selectedPoints`     | List of currently selected points (read-only)                      | Not supported                                      |
| `events`             | Event handlers (Select, Hover, Clear, Legend Click, etc.)           | Not supported (no chart event handlers)            |
| `clearOnEmptyData`   | Clear chart when data is empty                                     | Not supported                                      |
| `yAxis.scale`        | Axis scale: linear, log, time                                      | Not supported                                      |
| `yAxis.rangeMode`    | Axis range: auto or manual (with rangeMin/rangeMax)                | Not supported                                      |
| `yAxis.showGridLines`| Show grid lines on axis                                            | `.CartesianGrid(new CartesianGrid())`               |
| `yAxis.tickFormat`   | Format of axis tick labels                                         | Not supported                                      |
| `yAxis.title`        | Axis title                                                         | `.XAxis(new XAxis("label"))`                        |
| `yAxis.sort`         | Sort axis: none, ascending, descending                             | Not supported (sort data before passing)           |
| `scrollIntoView()`   | Method to scroll component into view                               | Not supported                                      |
| `setHidden()`        | Method to toggle visibility                                        | Not supported                                      |
