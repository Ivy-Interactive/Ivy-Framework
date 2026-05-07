# Radar Chart

A radar chart displays multivariate data of three or more quantitative variables mapped onto axes radiating from a central point, allowing comparison of multiple data series in a polar coordinate system.

## Reflex

```python
def radar_simple():
    return rx.recharts.radar_chart(
        rx.recharts.radar(
            data_key="A",
            stroke="#8884d8",
            fill="#8884d8",
        ),
        rx.recharts.polar_grid(),
        rx.recharts.polar_angle_axis(data_key="subject"),
        rx.recharts.polar_radius_axis(angle=90, domain=[0, 150]),
        data=data,
        width="100%",
        height=300,
    )
```

## Ivy

Ivy does not have a RadarChart widget. The supported chart types are:

- `LineChart` — via `.ToLineChart()`
- `BarChart` — via `.ToBarChart()`
- `AreaChart` — via `.ToAreaChart()`
- `PieChart` — via `.ToPieChart()`

```csharp
// No RadarChart equivalent exists in Ivy.
// Closest alternative: use a LineChart or BarChart to display multivariate data.
data.ToLineChart()
    .Dimension("Subject", e => e.Subject)
    .Measure("A", e => e.Sum(f => f.A))
    .Legend()
    .Tooltip()
    .Height(300);
```

## Parameters

### rx.recharts.RadarChart

| Parameter      | Documentation                                                    | Ivy           |
|----------------|------------------------------------------------------------------|---------------|
| data           | `Sequence` — the data source for the chart                      | Not supported |
| margin         | `Dict[str, Any]` — chart margins (top, right, left, bottom)     | Not supported |
| cx             | `Union[str, int]` — x-coordinate of the center, default `"50%"` | Not supported |
| cy             | `Union[str, int]` — y-coordinate of the center, default `"50%"` | Not supported |
| start_angle    | `int` — start angle of the chart, default `90`                  | Not supported |
| end_angle      | `int` — end angle of the chart, default `-270`                  | Not supported |
| inner_radius   | `Union[str, int]` — inner radius, default `0`                   | Not supported |
| outer_radius   | `Union[str, int]` — outer radius, default `"80%"`               | Not supported |
| width          | `Union[str, int]` — chart width, default `"100%"`               | `.Width()`    |
| height         | `Union[str, int]` — chart height, default `"100%"`              | `.Height()`   |

### rx.recharts.Radar

| Parameter          | Documentation                                                                  | Ivy           |
|--------------------|--------------------------------------------------------------------------------|---------------|
| data_key           | `Union[str, int]` — key in the data to plot                                   | `.Measure()`  |
| dot                | `Union[dict, bool]` — show dots at data vertices, default `True`              | Not supported |
| stroke             | `Union[str, Color]` — stroke color                                            | `.ColorScheme()` |
| fill               | `Union[str, Color]` — fill color                                              | `.ColorScheme()` |
| fill_opacity       | `float` — fill opacity, default `0.6`                                         | Not supported |
| legend_type        | `str` — legend icon type (`"circle"`, `"cross"`, `"rect"`, etc.)              | Not supported |
| label              | `Union[dict, bool]` — show labels, default `True`                             | Not supported |
| is_animation_active| `bool` — enable animation, default `True` (CSR) / `False` (SSR)              | Not supported |
| animation_begin    | `int` — animation delay in ms, default `0`                                    | Not supported |
| animation_duration | `int` — animation duration in ms, default `1500`                              | Not supported |
| animation_easing   | `str` — easing function (`"ease"`, `"ease-in"`, `"ease-out"`, `"linear"`)     | Not supported |

### Supporting Components

| Component              | Documentation                                     | Ivy              |
|------------------------|----------------------------------------------------|------------------|
| PolarAngleAxis         | Defines the angular axis (categories)              | `.Dimension()`   |
| PolarRadiusAxis        | Defines the radial axis (values)                   | `.YAxis()`       |
| PolarGrid              | Renders the polar grid lines                       | `.CartesianGrid()` |
| Legend                  | Displays the chart legend                          | `.Legend()`       |
| GraphingTooltip        | Shows tooltip on hover                             | `.Tooltip()`     |
