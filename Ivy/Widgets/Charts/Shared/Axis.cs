

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public enum AxisScales { Auto, Linear, Pow, Sqrt, Log, Identity, Time, Band, Point, Ordinal, Quantile, Quantize, Utc, Sequential, Threshold }
public enum AxisTypes { Category, Number }

public abstract record AxisBase<T> where T : AxisBase<T>
{
    public AxisBase(string? dataKey)
    {
        this.DataKey = dataKey;
    }

    public string? DataKey { get; init; }
    public AxisTypes Type { get; set; } = AxisTypes.Category;
    public AxisScales Scale { get; set; } = AxisScales.Auto;
    public bool AllowDecimals { get; set; } = true;
    public bool AllowDuplicatedCategory { get; set; } = true;
    public bool AllowDataOverflow { get; set; } = false;
    public double Angle { get; set; } = 0;
    public int TickCount { get; set; } = 5;
    public int TickSize { get; set; } = 6;
    public bool IncludeHidden { get; set; } = false;
    public string? Name { get; set; } = null;
    public string? Unit { get; set; } = null;
    public string? Label { get; set; } = null;
    public bool Reversed { get; set; } = false;
    public bool Mirror { get; set; } = false;
    public object DomainStart { get; set; } = "auto";
    public object DomainEnd { get; set; } = "auto";
    public bool TickLine { get; set; } = false;
    public bool AxisLine { get; set; } = true;
    public int MinTickGap { get; set; } = 5;
    public bool Hide { get; init; } = false;
}

public record XAxis : AxisBase<XAxis>
{
    public enum Orientations { Top, Bottom }
    public int Height { get; set; } = 30;

    public XAxis(string? dataKey = null) : base(dataKey)
    {
        Type = AxisTypes.Category;
    }

    public Orientations Orientation { get; set; } = Orientations.Bottom;
}

public record YAxis : AxisBase<YAxis>
{
    public enum Orientations { Left, Right }
    public int Width { get; set; } = 60;

    public YAxis(string? dataKey = null) : base(dataKey)
    {
        Type = AxisTypes.Number;
    }

    public Orientations Orientation { get; set; } = Orientations.Left;
}

public static class AxisExtensions
{
    public static XAxis Orientation(this XAxis axis, XAxis.Orientations orientation)
    {
        return axis with { Orientation = orientation };
    }

    public static YAxis Orientation(this YAxis axis, YAxis.Orientations orientation)
    {
        return axis with { Orientation = orientation };
    }

    public static T Type<T>(this T axis, AxisTypes type) where T : AxisBase<T>
    {
        return axis with { Type = type };
    }

    public static T AllowDecimals<T>(this T axis, bool allowDecimals) where T : AxisBase<T>
    {
        return axis with { AllowDecimals = allowDecimals };
    }

    public static T AllowDuplicatedCategory<T>(this T axis, bool allowDuplicatedCategory) where T : AxisBase<T>
    {
        return axis with { AllowDuplicatedCategory = allowDuplicatedCategory };
    }

    public static T AllowDataOverflow<T>(this T axis, bool allowDataOverflow) where T : AxisBase<T>
    {
        return axis with { AllowDataOverflow = allowDataOverflow };
    }

    public static T Angle<T>(this T axis, double angle) where T : AxisBase<T>
    {
        return axis with { Angle = angle };
    }

    public static T TickCount<T>(this T axis, int tickCount) where T : AxisBase<T>
    {
        return axis with { TickCount = tickCount };
    }

    public static T IncludeHidden<T>(this T axis, bool includeHidden) where T : AxisBase<T>
    {
        return axis with { IncludeHidden = includeHidden };
    }

    public static T Name<T>(this T axis, string name) where T : AxisBase<T>
    {
        return axis with { Name = name };
    }

    public static T Unit<T>(this T axis, string unit) where T : AxisBase<T>
    {
        return axis with { Unit = unit };
    }

    public static T Label<T>(this T axis, string label) where T : AxisBase<T>
    {
        return axis with { Label = label };
    }

    public static T Reversed<T>(this T axis, bool reversed = true) where T : AxisBase<T>
    {
        return axis with { Reversed = reversed };
    }

    public static T Mirror<T>(this T axis, bool mirror = true) where T : AxisBase<T>
    {
        return axis with { Mirror = mirror };
    }

    public static T Scale<T>(this T axis, AxisScales scale) where T : AxisBase<T>
    {
        return axis with { Scale = scale };
    }

    public static T TickSize<T>(this T axis, int tickSize) where T : AxisBase<T>
    {
        return axis with { TickSize = tickSize };
    }

    public static T Domain<T>(this T axis, object start, object end) where T : AxisBase<T>
    {
        return axis with { DomainStart = start, DomainEnd = end };
    }

    public static T TickLine<T>(this T axis, bool tickLine = true) where T : AxisBase<T>
    {
        return axis with { TickLine = tickLine };
    }

    public static T AxisLine<T>(this T axis, bool axisLine = true) where T : AxisBase<T>
    {
        return axis with { AxisLine = axisLine };
    }

    public static XAxis Height(this XAxis axis, int height)
    {
        return axis with { Height = height };
    }

    public static YAxis Width(this YAxis axis, int width)
    {
        return axis with { Width = width };
    }

    public static T MinTickGap<T>(this T axis, int minTickGap) where T : AxisBase<T>
    {
        return axis with { MinTickGap = minTickGap };
    }

    public static T Hide<T>(this T axis, bool hide = true) where T : AxisBase<T>
    {
        return axis with { Hide = hide };
    }
}