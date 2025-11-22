
// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public enum ColorScheme
{
    Default,
    Rainbow
}

public enum Positions
{
    Top,
    Left,
    Right,
    Bottom,
    Inside,
    Outside,
    InsideLeft,
    InsideRight,
    InsideTop,
    InsideBottom,
    InsideTopLeft,
    InsideBottomLeft,
    InsideTopRight,
    InsideBottomRight,
    /// <summary>At the start of the data flow (left for LTR, right for RTL).</summary>
    InsideStart,
    /// <summary>At the end of the data flow (right for LTR, left for RTL).</summary>
    InsideEnd,
    End,
    Center
}

public enum Layouts
{
    Horizontal,
    Vertical
}

public enum CurveTypes
{
    Basis,
    BasisClosed,
    BasisOpen,
    BumpX,
    BumpY,
    Bump,
    Linear,
    LinearClosed,
    Natural,
    MonotoneX,
    MonotoneY,
    Monotone,
    Step,
    StepBefore,
    StepAfter
}

public enum LegendTypes
{
    Line,
    PlainLine,
    Square,
    Rect,
    Circle,
    Cross,
    Diamond,
    Star,
    Triangle,
    Wye,
    None
}

public enum Scales
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

public enum StackOffsetTypes
{
    /// <summary>Stacks series and normalizes them to fill the full height (0-100%).</summary>
    Expand,
    None,
    /// <summary>Stacks series with a wiggle effect that minimizes the change in slope.</summary>
    Wiggle,
    /// <summary>Stacks series and centers them around the middle line for balanced visualization.</summary>
    Silhouette
}