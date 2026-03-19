using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A circular gauge/dial widget for displaying KPI values such as progress, completion, or load.
/// </summary>
public record GaugeChart : WidgetBase<GaugeChart>
{
    public GaugeChart(double value) : this()
    {
        Value = value;
    }

    internal GaugeChart()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public double Value { get; init; }

    [Prop] public double Min { get; init; } = 0;

    [Prop] public double Max { get; init; } = 100;

    [Prop] public string? Label { get; init; }

    [Prop] public int StartAngle { get; init; } = 225;

    [Prop] public int EndAngle { get; init; } = -45;

    [Prop] public GaugeThreshold[] Thresholds { get; init; } = [];

    [Prop] public GaugePointer? Pointer { get; init; }

    [Prop] public bool Animated { get; init; } = true;

    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;

    public static GaugeChart operator |(GaugeChart widget, object child)
    {
        throw new NotSupportedException("GaugeChart does not support children.");
    }
}

public static partial class GaugeChartExtensions
{
    public static GaugeChart Min(this GaugeChart chart, double min)
    {
        return chart with { Min = min };
    }

    public static GaugeChart Max(this GaugeChart chart, double max)
    {
        return chart with { Max = max };
    }

    public static GaugeChart Label(this GaugeChart chart, string label)
    {
        return chart with { Label = label };
    }

    public static GaugeChart StartAngle(this GaugeChart chart, int startAngle)
    {
        return chart with { StartAngle = startAngle };
    }

    public static GaugeChart EndAngle(this GaugeChart chart, int endAngle)
    {
        return chart with { EndAngle = endAngle };
    }

    public static GaugeChart Thresholds(this GaugeChart chart, params GaugeThreshold[] thresholds)
    {
        return chart with { Thresholds = thresholds };
    }

    public static GaugeChart Pointer(this GaugeChart chart, GaugePointer pointer)
    {
        return chart with { Pointer = pointer };
    }

    public static GaugeChart Pointer(this GaugeChart chart)
    {
        return chart with { Pointer = new GaugePointer() };
    }

    public static GaugeChart Animated(this GaugeChart chart, bool animated = true)
    {
        return chart with { Animated = animated };
    }

    public static GaugeChart ColorScheme(this GaugeChart chart, ColorScheme colorScheme)
    {
        return chart with { ColorScheme = colorScheme };
    }
}
