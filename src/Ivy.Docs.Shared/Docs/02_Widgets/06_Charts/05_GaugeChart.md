---
searchHints:
  - visualization
  - gauge
  - dial
  - kpi
  - dashboard
  - meter
  - speedometer
---

# GaugeChart

<Ingress>
Display KPI values such as progress, completion, or load with circular gauge/dial widgets powered by Apache ECharts.
</Ingress>

Gauge charts are ideal for dashboards where a single numeric value needs to be shown relative to a range. Build gauge [views](../../01_Onboarding/02_Concepts/02_Views.md) inside [layouts](../../01_Onboarding/02_Concepts/04_Layout.md) and use [state](../../03_Hooks/02_Core/03_UseState.md) for dynamic updates. See [Charts](../../01_Onboarding/02_Concepts/18_Charts.md) for an overview of Ivy chart widgets.

```csharp demo-tabs
public class BasicGaugeDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | new GaugeChart(75)
                .Label("Completion")
                .Height(Size.Px(300));
    }
}
```

## Color Thresholds

Use `Thresholds` to define color ranges on the gauge axis. Each `GaugeThreshold` specifies a value boundary and a color. The gauge arc is colored according to these ranges.

```csharp demo-tabs
public class GaugeThresholdsDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | new GaugeChart(65)
                .Thresholds(
                    new GaugeThreshold(40, "#ef4444"),
                    new GaugeThreshold(70, "#eab308"),
                    new GaugeThreshold(100, "#22c55e"))
                .Label("Performance")
                .Pointer()
                .Height(Size.Px(300));
    }
}
```

## Custom Range

By default the gauge ranges from 0 to 100. Use `Min` and `Max` to set a custom range.

```csharp demo-tabs
public class GaugeCustomRangeDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | new GaugeChart(150)
                .Min(0)
                .Max(200)
                .Label("Speed (km/h)")
                .Pointer()
                .Height(Size.Px(300));
    }
}
```

## Semicircular Gauge

Adjust `StartAngle` and `EndAngle` to create a semicircular gauge.

```csharp demo-tabs
public class GaugeSemicircleDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | new GaugeChart(72)
                .StartAngle(180)
                .EndAngle(0)
                .Label("CPU Usage")
                .Pointer()
                .Height(Size.Px(300));
    }
}
```

## Pointer Styles

The gauge supports three pointer styles: `Arrow` (default), `Line`, and `Rounded`. Use the `Pointer` method with a `GaugePointer` record to configure style, width, and length.

```csharp demo-tabs
public class GaugePointerStylesDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(3)
            | (new GaugeChart(60)
                .Pointer(new GaugePointer { Style = GaugePointerStyle.Arrow })
                .Label("Arrow")
                .Height(Size.Px(250)))
            | (new GaugeChart(60)
                .Pointer(new GaugePointer { Style = GaugePointerStyle.Line })
                .Label("Line")
                .Height(Size.Px(250)))
            | (new GaugeChart(60)
                .Pointer(new GaugePointer { Style = GaugePointerStyle.Rounded })
                .Label("Rounded")
                .Height(Size.Px(250)));
    }
}
```

## Dynamic Updates

Bind gauge value to state for real-time updates. The gauge animates smoothly between values by default.

```csharp demo-tabs
public class GaugeDynamicDemo : ViewBase
{
    public override object? Build()
    {
        var value = UseState(50.0);

        return Layout.Vertical()
            | new GaugeChart(value.Value)
                .Thresholds(
                    new GaugeThreshold(30, "#ef4444"),
                    new GaugeThreshold(60, "#eab308"),
                    new GaugeThreshold(100, "#22c55e"))
                .Label($"{value.Value}%")
                .Pointer()
                .Height(Size.Px(300))
            | (Layout.Horizontal()
                | new Button("-10", _ => value.Set(Math.Max(0, value.Value - 10)))
                | new Button("+10", _ => value.Set(Math.Min(100, value.Value + 10))));
    }
}
```

## KPI Dashboard

Gauge charts work well in grid layouts for KPI dashboards.

```csharp demo-tabs
public class GaugeDashboardDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(3)
            | (new Card(
                new GaugeChart(82)
                    .Thresholds(
                        new GaugeThreshold(50, "#ef4444"),
                        new GaugeThreshold(80, "#eab308"),
                        new GaugeThreshold(100, "#22c55e"))
                    .Label("82%")
                    .Pointer()
                    .Height(Size.Px(200))).Title("Sales Target"))
            | (new Card(
                new GaugeChart(45)
                    .Thresholds(
                        new GaugeThreshold(60, "#22c55e"),
                        new GaugeThreshold(80, "#eab308"),
                        new GaugeThreshold(100, "#ef4444"))
                    .Label("45%")
                    .Pointer()
                    .Height(Size.Px(200))).Title("Server Load"))
            | (new Card(
                new GaugeChart(91)
                    .Thresholds(
                        new GaugeThreshold(70, "#22c55e"),
                        new GaugeThreshold(90, "#eab308"),
                        new GaugeThreshold(100, "#ef4444"))
                    .Label("91%")
                    .Pointer()
                    .Height(Size.Px(200))).Title("Disk Usage"));
    }
}
```

<WidgetDocs Type="Ivy.GaugeChart" ExtensionTypes="Ivy.GaugeChartExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Charts/GaugeChart.cs"/>
