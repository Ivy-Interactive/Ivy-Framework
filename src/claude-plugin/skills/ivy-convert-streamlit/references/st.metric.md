# st.metric

Display a metric in big bold font, with an optional indicator of how the metric changed.

## Streamlit

```python
import streamlit as st

col1, col2, col3 = st.columns(3)
col1.metric("Temperature", "70 °F", "1.2 °F")
col2.metric("Wind", "9 mph", "-8%")
col3.metric("Humidity", "86%", "4%")
```

## Ivy

```csharp
new MetricView(
    "Total Sales",
    Icons.DollarSign,
    ctx => ctx.UseQuery(
        key: "total-sales",
        fetcher: () => Task.FromResult(new MetricRecord(
            MetricFormatted: "$84,250",
            TrendComparedToPreviousPeriod: 0.21m,
            GoalAchieved: 0.75m,
            GoalFormatted: "$800,000"
        ))
    )
)
```

## Parameters

| Parameter          | Documentation                                                                                                  | Ivy                                              |
|--------------------|----------------------------------------------------------------------------------------------------------------|--------------------------------------------------|
| `label`            | Header/title shown above the metric value. Supports GitHub-flavored Markdown.                                  | `title` constructor parameter                    |
| `value`            | Main metric value displayed in big bold font. Can be int, float, str, or None.                                 | `MetricRecord.MetricFormatted`                   |
| `delta`            | Change indicator shown below the value with a directional arrow. Negative values get a down arrow.             | `MetricRecord.TrendComparedToPreviousPeriod`     |
| `delta_color`      | Color of the delta indicator: `"normal"`, `"inverse"`, `"off"`, or a named color.                              | Not supported (auto green/red based on sign)     |
| `help`             | Tooltip text displayed next to the label.                                                                      | Not supported                                    |
| `label_visibility` | Controls label visibility: `"visible"`, `"hidden"`, or `"collapsed"`.                                          | Not supported                                    |
| `border`           | Whether to show a border around the metric.                                                                    | Not supported (always rendered as a Card)        |
| `chart_data`       | Numeric sequence rendered as a sparkline chart below the metric.                                               | Not supported                                    |
| `chart_type`       | Sparkline chart style: `"line"`, `"area"`, or `"bar"`.                                                         | Not supported                                    |
| `delta_arrow`      | Arrow direction override: `"auto"`, `"up"`, `"down"`, or `"off"`.                                              | Not supported (always auto)                      |
| `format`           | Number formatting string: `"compact"`, `"percent"`, `"dollar"`, printf-style, etc.                             | Not supported (pre-format via `MetricFormatted`) |
| —                  | —                                                                                                              | `icon` — optional icon displayed next to title   |
| —                  | —                                                                                                              | `MetricRecord.GoalAchieved` — progress ratio     |
| —                  | —                                                                                                              | `MetricRecord.GoalFormatted` — goal target text  |
