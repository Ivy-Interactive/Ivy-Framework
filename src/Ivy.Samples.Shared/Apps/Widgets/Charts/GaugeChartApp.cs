
namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.Gauge, searchHints: ["visualization", "gauge", "dial", "kpi", "dashboard", "meter", "progress", "speedometer"])]
public class GaugeChartApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Grid().Columns(3)
            | new GaugeChart0View()
            | new GaugeChart1View()
            | new GaugeChart2View()
            | new GaugeChart3View()
            | new GaugeChart4View()
            | new GaugeChart5View()
            | new GaugeChart6View()
            | new GaugeChart7View()
            | new GaugeChart8View()
        ;
    }
}

public class GaugeChart0View : ViewBase
{
    public override object? Build()
    {
        return new Card().Title("Basic Gauge")
            | new GaugeChart(75)
                .Label("Completion")
                .Height(Size.Px(250));
    }
}

public class GaugeChart1View : ViewBase
{
    public override object? Build()
    {
        return new Card().Title("Color Thresholds")
            | new GaugeChart(65)
                .Thresholds(
                    new GaugeThreshold(40, "#ef4444"),
                    new GaugeThreshold(70, "#eab308"),
                    new GaugeThreshold(100, "#22c55e"))
                .Label("Performance")
                .Pointer()
                .Height(Size.Px(250));
    }
}

public class GaugeChart2View : ViewBase
{
    public override object? Build()
    {
        return new Card().Title("Custom Range (0-200)")
            | new GaugeChart(150)
                .Min(0)
                .Max(200)
                .Label("Speed")
                .Pointer()
                .Height(Size.Px(250));
    }
}

public class GaugeChart3View : ViewBase
{
    public override object? Build()
    {
        return new Card().Title("Semicircular Gauge")
            | new GaugeChart(72)
                .StartAngle(180)
                .EndAngle(0)
                .Label("CPU Usage")
                .Pointer()
                .Height(Size.Px(250));
    }
}

public class GaugeChart4View : ViewBase
{
    public override object? Build()
    {
        return new Card().Title("Arrow Pointer")
            | new GaugeChart(60)
                .Pointer(new GaugePointer { Style = GaugePointerStyle.Arrow })
                .Label("Arrow")
                .Height(Size.Px(250));
    }
}

public class GaugeChart5View : ViewBase
{
    public override object? Build()
    {
        return new Card().Title("Line Pointer")
            | new GaugeChart(60)
                .Pointer(new GaugePointer { Style = GaugePointerStyle.Line })
                .Label("Line")
                .Height(Size.Px(250));
    }
}

public class GaugeChart6View : ViewBase
{
    public override object? Build()
    {
        return new Card().Title("Rounded Pointer")
            | new GaugeChart(60)
                .Pointer(new GaugePointer { Style = GaugePointerStyle.Rounded })
                .Label("Rounded")
                .Height(Size.Px(250));
    }
}

public class GaugeChart7View : ViewBase
{
    public override object? Build()
    {
        var value = UseState(50.0);

        return new Card().Title("Interactive Gauge")
            | (Layout.Vertical()
                | (new GaugeChart(value.Value)
                    .Thresholds(
                        new GaugeThreshold(30, "#ef4444"),
                        new GaugeThreshold(60, "#eab308"),
                        new GaugeThreshold(100, "#22c55e"))
                    .Label($"{value.Value}%")
                    .Pointer()
                    .Height(Size.Px(200)))
                | (Layout.Horizontal()
                    | new Button("-10", _ => value.Set(Math.Max(0, value.Value - 10)))
                    | new Button("+10", _ => value.Set(Math.Min(100, value.Value + 10)))));
    }
}

public class GaugeChart8View : ViewBase
{
    public override object? Build()
    {
        return new Card().Title("No Animation")
            | new GaugeChart(85)
                .Animated(false)
                .Label("Static")
                .Height(Size.Px(250));
    }
}
