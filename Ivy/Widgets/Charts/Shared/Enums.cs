
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
    InsideStart,
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
    Expand,
    None,
    Wiggle,
    Silhouette
}