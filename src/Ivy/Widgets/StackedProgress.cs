using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A segment within a stacked progress bar.
/// </summary>
public record ProgressSegment(double Value, Colors? Color = null, string? Label = null);

/// <summary>
/// A progress bar that displays multiple colored segments side by side, each proportional to its value.
/// </summary>
public record StackedProgress : WidgetBase<StackedProgress>
{
    public StackedProgress(params ProgressSegment[] segments) : this()
    {
        Segments = segments;
    }

    internal StackedProgress()
    {
        Width = Size.Full();
    }

    [Prop] public ProgressSegment[] Segments { get; set; } = [];
    [Prop] public double BarHeight { get; set; } = 8;
    [Prop] public bool ShowLabels { get; set; }
    [Prop] public bool Rounded { get; set; } = true;

    public static StackedProgress operator |(StackedProgress widget, object child)
    {
        throw new NotSupportedException("StackedProgress does not support children.");
    }
}

public static class StackedProgressExtensions
{
    public static StackedProgress BarHeight(this StackedProgress w, double height) => w with { BarHeight = height };
    public static StackedProgress ShowLabels(this StackedProgress w, bool show = true) => w with { ShowLabels = show };
    public static StackedProgress Rounded(this StackedProgress w, bool rounded = true) => w with { Rounded = rounded };
}
