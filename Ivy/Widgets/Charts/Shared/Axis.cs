

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public enum AxisScales
{
    Auto,
    Linear,
    Pow,
    Sqrt,
    Log,
    Identity,
    Time,
    Band,
    Point,
    Ordinal,
    Quantile,
    Quantize,
    Utc,
    Sequential,
    Threshold
}

public enum AxisTypes
{
    Category,
    Number
}

public abstract record AxisBase<T> where T : AxisBase<T>
{
    public AxisBase(string? dataKey)
    {
        this.DataKey = dataKey;
    }

    public string? DataKey { get; init; }

    public AxisTypes Type { get; set; } = AxisTypes.Category;

    public AxisScales Scale { get; set; } = AxisScales.Auto;

    /// <summary>When false, only integer values are displayed.</summary>
    public bool AllowDecimals { get; set; } = true;

    /// <summary>When false, duplicate categories will be merged.</summary>
    public bool AllowDuplicatedCategory { get; set; } = true;

    public bool AllowDataOverflow { get; set; } = false;

    /// <summary>Positive values rotate clockwise.</summary>
    public double Angle { get; set; } = 0;

    /// <summary>The actual number may be adjusted for optimal spacing.</summary>
    public int TickCount { get; set; } = 5;

    public int TickSize { get; set; } = 6;

    /// <summary>When true, hidden data affects the axis range.</summary>
    public bool IncludeHidden { get; set; } = false;

    public string? Name { get; set; } = null;

    public string? Unit { get; set; } = null;

    /// <summary>This is separate from the Name property and is shown directly on the axis.</summary>
    public string? Label { get; set; } = null;

    /// <summary>When true, the highest value appears at the start.</summary>
    public bool Reversed { get; set; } = false;

    /// <summary>Useful for creating symmetric charts.</summary>
    public bool Mirror { get; set; } = false;

    /// <summary>Use "auto" for automatic calculation or specify a specific value.</summary>
    public object DomainStart { get; set; } = "auto";

    /// <summary>Use "auto" for automatic calculation or specify a specific value.</summary>
    public object DomainEnd { get; set; } = "auto";

    public bool TickLine { get; set; } = false;

    public bool AxisLine { get; set; } = true;

    /// <summary>This ensures tick labels don't overlap.</summary>
    public int MinTickGap { get; set; } = 5;

    /// <summary>When true, the axis and all its elements are invisible.</summary>
    public bool Hide { get; init; } = false;
}

public record XAxis : AxisBase<XAxis>
{
    public enum Orientations
    {
        Top,
        Bottom
    }

    public int Height { get; set; } = 30;

    /// <summary>The X-axis defaults to Category type for better handling of text-based data.</summary>
    /// <param name="dataKey">If null, the axis will not be bound to specific data.</param>
    public XAxis(string? dataKey = null) : base(dataKey)
    {
        Type = AxisTypes.Category;
    }

    public Orientations Orientation { get; set; } = Orientations.Bottom;
}

public record YAxis : AxisBase<YAxis>
{
    public enum Orientations
    {
        Left,
        Right
    }

    public int Width { get; set; } = 60;

    /// <summary>The Y-axis defaults to Number type for better handling of numerical data.</summary>
    /// <param name="dataKey">If null, the axis will not be bound to specific data.</param>
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

    /// <param name="angle">Positive values rotate clockwise.</param>
    public static T Angle<T>(this T axis, double angle) where T : AxisBase<T>
    {
        return axis with { Angle = angle };
    }

    /// <param name="tickCount">The actual number may be adjusted for optimal spacing.</param>
    public static T TickCount<T>(this T axis, int tickCount) where T : AxisBase<T>
    {
        return axis with { TickCount = tickCount };
    }

    /// <param name="includeHidden">When true, hidden data affects the axis range.</param>
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

    /// <summary>This is separate from the Name property and is shown directly on the axis.</summary>
    public static T Label<T>(this T axis, string label) where T : AxisBase<T>
    {
        return axis with { Label = label };
    }

    /// <param name="reversed">When true, the highest value appears at the start.</param>
    public static T Reversed<T>(this T axis, bool reversed = true) where T : AxisBase<T>
    {
        return axis with { Reversed = reversed };
    }

    /// <summary>Useful for creating symmetric charts.</summary>
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

    /// <param name="start">Use "auto" for automatic calculation or specify specific values.</param>
    /// <param name="end">Use "auto" for automatic calculation or specify specific values.</param>
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

    /// <summary>This ensures tick labels don't overlap.</summary>
    public static T MinTickGap<T>(this T axis, int minTickGap) where T : AxisBase<T>
    {
        return axis with { MinTickGap = minTickGap };
    }

    /// <param name="hide">When true, the axis and all its elements are invisible.</param>
    public static T Hide<T>(this T axis, bool hide = true) where T : AxisBase<T>
    {
        return axis with { Hide = hide };
    }
}