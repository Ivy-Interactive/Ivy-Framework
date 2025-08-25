
// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Defines the available color schemes for charts. Color schemes determine how colors are automatically
/// assigned to different data series and chart elements.
/// </summary>
public enum ColorScheme
{
    /// <summary>Uses a predefined set of colors that provide good contrast and accessibility.</summary>
    Default,
    /// <summary>Uses a rainbow-like color palette with vibrant, distinct colors for each series.</summary>
    Rainbow
}

/// <summary>
/// Defines the possible positions for chart elements such as legends, labels, and tooltips.
/// These positions can be used to control the placement of various chart components.
/// </summary>
public enum Positions
{
    /// <summary>Positioned at the top of the chart or container.</summary>
    Top,
    /// <summary>Positioned at the left side of the chart or container.</summary>
    Left,
    /// <summary>Positioned at the right side of the chart or container.</summary>
    Right,
    /// <summary>Positioned at the bottom of the chart or container.</summary>
    Bottom,
    /// <summary>Positioned inside the chart area, typically centered.</summary>
    Inside,
    /// <summary>Positioned outside the chart area, typically in the margin.</summary>
    Outside,
    /// <summary>Positioned inside the chart area, aligned to the left.</summary>
    InsideLeft,
    /// <summary>Positioned inside the chart area, aligned to the right.</summary>
    InsideRight,
    /// <summary>Positioned inside the chart area, aligned to the top.</summary>
    InsideTop,
    /// <summary>Positioned inside the chart area, aligned to the bottom.</summary>
    InsideBottom,
    /// <summary>Positioned inside the chart area, at the top-left corner.</summary>
    InsideTopLeft,
    /// <summary>Positioned inside the chart area, at the bottom-left corner.</summary>
    InsideBottomLeft,
    /// <summary>Positioned inside the chart area, at the top-right corner.</summary>
    InsideTopRight,
    /// <summary>Positioned inside the chart area, at the bottom-right corner.</summary>
    InsideBottomRight,
    /// <summary>Positioned inside the chart area, at the start of the data flow (left for LTR, right for RTL).</summary>
    InsideStart,
    /// <summary>Positioned inside the chart area, at the end of the data flow (right for LTR, left for RTL).</summary>
    InsideEnd,
    /// <summary>Positioned at the end of the chart or container.</summary>
    End,
    /// <summary>Positioned at the center of the chart or container.</summary>
    Center
}

/// <summary>
/// Defines the layout orientation for charts and chart elements.
/// </summary>
public enum Layouts
{
    /// <summary>Chart elements are arranged horizontally, from left to right.</summary>
    Horizontal,
    /// <summary>Chart elements are arranged vertically, from top to bottom.</summary>
    Vertical
}

/// <summary>
/// Defines the types of curves used to connect data points in line and area charts.
/// </summary>
public enum CurveTypes
{
    /// <summary>Basis spline curve that creates smooth, natural-looking curves through data points.</summary>
    Basis,
    /// <summary>Closed basis spline curve that connects the last point back to the first point.</summary>
    BasisClosed,
    /// <summary>Open basis spline curve that doesn't connect the last point to the first.</summary>
    BasisOpen,
    /// <summary>Bump curve that creates smooth transitions with horizontal emphasis.</summary>
    BumpX,
    /// <summary>Bump curve that creates smooth transitions with vertical emphasis.</summary>
    BumpY,
    /// <summary>Bump curve that creates smooth transitions in both directions.</summary>
    Bump,
    /// <summary>Straight line connections between data points.</summary>
    Linear,
    /// <summary>Closed straight line connections that form a complete polygon.</summary>
    LinearClosed,
    /// <summary>Natural cubic spline that creates the smoothest possible curve through data points.</summary>
    Natural,
    /// <summary>Monotone curve that preserves the monotonicity of data in the X direction.</summary>
    MonotoneX,
    /// <summary>Monotone curve that preserves the monotonicity of data in the Y direction.</summary>
    MonotoneY,
    /// <summary>Monotone curve that preserves the monotonicity of data in both directions.</summary>
    Monotone,
    /// <summary>Step curve that creates horizontal steps at each data point.</summary>
    Step,
    /// <summary>Step curve that creates horizontal steps before each data point.</summary>
    StepBefore,
    /// <summary>Step curve that creates horizontal steps after each data point.</summary>
    StepAfter
}

/// <summary>
/// Defines the visual representation types for chart legends.
/// </summary>
public enum LegendTypes
{
    /// <summary>Line representation in the legend, typically a short line segment.</summary>
    Line,
    /// <summary>Plain line representation without additional styling.</summary>
    PlainLine,
    /// <summary>Square representation in the legend.</summary>
    Square,
    /// <summary>Rectangle representation in the legend.</summary>
    Rect,
    /// <summary>Circle representation in the legend.</summary>
    Circle,
    /// <summary>Cross (X) representation in the legend.</summary>
    Cross,
    /// <summary>Diamond representation in the legend.</summary>
    Diamond,
    /// <summary>Star representation in the legend.</summary>
    Star,
    /// <summary>Triangle representation in the legend.</summary>
    Triangle,
    /// <summary>Wye (Y) representation in the legend.</summary>
    Wye,
    /// <summary>No legend representation, the series won't appear in the legend.</summary>
    None
}

/// <summary>
/// Defines the scale types available for chart axes and data transformations.
/// </summary>
public enum Scales
{
    /// <summary>Automatically determines the best scale based on data analysis.</summary>
    Auto,
    /// <summary>Linear scale for continuous numerical data with uniform spacing.</summary>
    Linear,
    /// <summary>Power scale for data that follows exponential patterns.</summary>
    Pow,
    /// <summary>Square root scale for data with diminishing returns.</summary>
    Sqrt,
    /// <summary>Logarithmic scale for data spanning multiple orders of magnitude.</summary>
    Log,
    /// <summary>Identity scale that maps values directly without transformation.</summary>
    Identity,
    /// <summary>Time scale for temporal data with automatic date formatting.</summary>
    Time,
    /// <summary>Band scale for categorical data with equal spacing.</summary>
    Band,
    /// <summary>Point scale for categorical data with centered positioning.</summary>
    Point,
    /// <summary>Ordinal scale for ordered categorical data.</summary>
    Ordinal,
    /// <summary>Quantile scale that divides data into equal-sized groups.</summary>
    Quantile,
    /// <summary>Quantize scale that divides the data range into equal intervals.</summary>
    Quantize,
    /// <summary>UTC time scale for coordinated universal time data.</summary>
    Utc,
    /// <summary>Sequential scale for continuous data with color mapping.</summary>
    Sequential,
    /// <summary>Threshold scale for data that crosses specific boundary values.</summary>
    Threshold
}

/// <summary>
/// Defines the stacking behavior for chart series.
/// </summary>
public enum StackOffsetTypes
{
    /// <summary>Stacks series and normalizes them to fill the full height (0-100%).</summary>
    Expand,
    /// <summary>No stacking applied, series are displayed independently.</summary>
    None,
    /// <summary>Stacks series with a wiggle effect that minimizes the change in slope.</summary>
    Wiggle,
    /// <summary>Stacks series and centers them around the middle line for balanced visualization.</summary>
    Silhouette
}