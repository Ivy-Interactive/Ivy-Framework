

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Defines the scale types available for chart axes. Different scales are used to transform data values
/// for appropriate visualization based on the data characteristics.
/// </summary>
public enum AxisScales
{
    /// <summary>Automatically determines the best scale based on data analysis.</summary>
    Auto,
    /// <summary>Linear scale for continuous numerical data.</summary>
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
/// Defines the data type classification for chart axes, determining how the axis processes and displays data.
/// </summary>
public enum AxisTypes
{
    /// <summary>Categorical data type for discrete, non-numerical values like names or labels.</summary>
    Category,
    /// <summary>Numerical data type for continuous or discrete numerical values.</summary>
    Number
}

/// <summary>
/// Abstract base class for chart axes that provides common functionality and properties for both X and Y axes.
/// This class implements the fluent API pattern for easy axis configuration.
/// </summary>
/// <typeparam name="T">The concrete axis type that inherits from this base class.</typeparam>
public abstract record AxisBase<T> where T : AxisBase<T>
{
    public AxisBase(string? dataKey)
    {
        this.DataKey = dataKey;
    }

    /// <summary>
    /// Gets or sets the key that identifies which data property this axis represents.
    /// This key should match a property name in your data objects.
    /// </summary>
    public string? DataKey { get; init; }

    /// <summary>
    /// Gets or sets the data type classification for this axis. Determines how the axis processes and displays data.
    /// </summary>
    public AxisTypes Type { get; set; } = AxisTypes.Category;

    /// <summary>
    /// Gets or sets the scale transformation applied to the axis data. Different scales are used to transform data values
    /// for appropriate visualization based on the data characteristics.
    /// </summary>
    public AxisScales Scale { get; set; } = AxisScales.Auto;

    /// <summary>
    /// Gets or sets whether decimal values are allowed on this axis. When false, only integer values are displayed.
    /// </summary>
    public bool AllowDecimals { get; set; } = true;

    /// <summary>
    /// Gets or sets whether duplicate category values are allowed. When false, duplicate categories will be merged.
    /// </summary>
    public bool AllowDuplicatedCategory { get; set; } = true;

    /// <summary>
    /// Gets or sets whether data points that exceed the axis domain are allowed to overflow beyond the chart boundaries.
    /// </summary>
    public bool AllowDataOverflow { get; set; } = false;

    /// <summary>
    /// Gets or sets the rotation angle of the axis labels in degrees. Positive values rotate clockwise.
    /// </summary>
    public double Angle { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of tick marks to display on the axis. The actual number may be adjusted for optimal spacing.
    /// </summary>
    public int TickCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets the size of tick marks in pixels. Tick marks are the small lines that extend from the axis.
    /// </summary>
    public int TickSize { get; set; } = 6;

    /// <summary>
    /// Gets or sets whether hidden data points are included in axis calculations. When true, hidden data affects the axis range.
    /// </summary>
    public bool IncludeHidden { get; set; } = false;

    /// <summary>
    /// Gets or sets the display name for this axis, typically shown in tooltips or legends.
    /// </summary>
    public string? Name { get; set; } = null;

    /// <summary>
    /// Gets or sets the unit of measurement for the axis values (e.g., "px", "kg", "%", "$").
    /// </summary>
    public string? Unit { get; set; } = null;

    /// <summary>
    /// Gets or sets the label text displayed on the axis. This is separate from the Name property and is shown directly on the axis.
    /// </summary>
    public string? Label { get; set; } = null;

    /// <summary>
    /// Gets or sets whether the axis values are displayed in reverse order. When true, the highest value appears at the start.
    /// </summary>
    public bool Reversed { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the axis is mirrored across the chart center. Useful for creating symmetric charts.
    /// </summary>
    public bool Mirror { get; set; } = false;

    /// <summary>
    /// Gets or sets the starting value for the axis domain. Use "auto" for automatic calculation or specify a specific value.
    /// </summary>
    public object DomainStart { get; set; } = "auto";

    /// <summary>
    /// Gets or sets the ending value for the axis domain. Use "auto" for automatic calculation or specify a specific value.
    /// </summary>
    public object DomainEnd { get; set; } = "auto";

    /// <summary>
    /// Gets or sets whether tick lines are displayed. Tick lines are the small lines that extend from the axis to help with value reading.
    /// </summary>
    public bool TickLine { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the main axis line is displayed. The axis line is the primary line that represents the axis.
    /// </summary>
    public bool AxisLine { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum gap between tick marks in pixels. This ensures tick labels don't overlap.
    /// </summary>
    public int MinTickGap { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether the axis is completely hidden from view. When true, the axis and all its elements are invisible.
    /// </summary>
    public bool Hide { get; init; } = false;
}

/// <summary>
/// Represents the X-axis (horizontal axis) of a chart. The X-axis typically displays categories, time periods, or independent variables.
/// Inherits all common axis functionality from AxisBase while providing X-axis specific properties.
/// </summary>
public record XAxis : AxisBase<XAxis>
{
    /// <summary>
    /// Defines the possible orientations for the X-axis, determining whether it appears above or below the chart.
    /// </summary>
    public enum Orientations
    {
        /// <summary>X-axis appears above the chart content.</summary>
        Top,
        /// <summary>X-axis appears below the chart content (default).</summary>
        Bottom
    }

    /// <summary>
    /// Gets or sets the height of the X-axis in pixels. This affects the space allocated for axis labels and tick marks.
    /// </summary>
    public int Height { get; set; } = 30;

    /// <summary>
    /// Initializes a new instance of the XAxis class with an optional data key.
    /// The X-axis defaults to Category type for better handling of text-based data.
    /// </summary>
    /// <param name="dataKey">The key that identifies which data property this X-axis represents. If null, the axis will not be bound to specific data.</param>
    public XAxis(string? dataKey = null) : base(dataKey)
    {
        Type = AxisTypes.Category;
    }

    /// <summary>
    /// Gets or sets the orientation of the X-axis, determining whether it appears above or below the chart.
    /// </summary>
    public Orientations Orientation { get; set; } = Orientations.Bottom;
}

/// <summary>
/// Represents the Y-axis (vertical axis) of a chart. The Y-axis typically displays numerical values, measurements, or dependent variables.
/// Inherits all common axis functionality from AxisBase while providing Y-axis specific properties.
/// </summary>
public record YAxis : AxisBase<YAxis>
{
    /// <summary>
    /// Defines the possible orientations for the Y-axis, determining whether it appears to the left or right of the chart.
    /// </summary>
    public enum Orientations
    {
        /// <summary>Y-axis appears to the left of the chart content (default).</summary>
        Left,
        /// <summary>Y-axis appears to the right of the chart content.</summary>
        Right
    }

    /// <summary>
    /// Gets or sets the width of the Y-axis in pixels. This affects the space allocated for axis labels and tick marks.
    /// </summary>
    public int Width { get; set; } = 60;

    /// <summary>
    /// Initializes a new instance of the YAxis class with an optional data key.
    /// The Y-axis defaults to Number type for better handling of numerical data.
    /// </summary>
    /// <param name="dataKey">The key that identifies which data property this Y-axis represents. If null, the axis will not be bound to specific data.</param>
    public YAxis(string? dataKey = null) : base(dataKey)
    {
        Type = AxisTypes.Number;
    }

    /// <summary>
    /// Gets or sets the orientation of the Y-axis, determining whether it appears to the left or right of the chart.
    /// </summary>
    public Orientations Orientation { get; set; } = Orientations.Left;
}

/// <summary>
/// Extension methods for chart axes that provide a fluent API for easy configuration.
/// These methods allow you to chain multiple configuration calls for better readability.
/// </summary>
public static class AxisExtensions
{
    /// <summary>
    /// Sets the orientation of the X-axis, determining whether it appears above or below the chart.
    /// </summary>
    /// <param name="axis">The X-axis to configure.</param>
    /// <param name="orientation">The desired orientation (Top or Bottom).</param>
    /// <returns>A new XAxis instance with the updated orientation.</returns>
    public static XAxis Orientation(this XAxis axis, XAxis.Orientations orientation)
    {
        return axis with { Orientation = orientation };
    }

    /// <summary>
    /// Sets the orientation of the Y-axis, determining whether it appears to the left or right of the chart.
    /// </summary>
    /// <param name="axis">The Y-axis to configure.</param>
    /// <param name="orientation">The desired orientation (Left or Right).</param>
    /// <returns>A new YAxis instance with the updated orientation.</returns>
    public static YAxis Orientation(this YAxis axis, YAxis.Orientations orientation)
    {
        return axis with { Orientation = orientation };
    }

    /// <summary>
    /// Sets the data type classification for the axis, determining how the axis processes and displays data.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="type">The data type (Category or Number).</param>
    /// <returns>A new axis instance with the updated data type.</returns>
    public static T Type<T>(this T axis, AxisTypes type) where T : AxisBase<T>
    {
        return axis with { Type = type };
    }

    /// <summary>
    /// Sets whether decimal values are allowed on the axis. When false, only integer values are displayed.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="allowDecimals">True to allow decimals, false to restrict to integers.</param>
    /// <returns>A new axis instance with the updated decimal allowance setting.</returns>
    public static T AllowDecimals<T>(this T axis, bool allowDecimals) where T : AxisBase<T>
    {
        return axis with { AllowDecimals = allowDecimals };
    }

    /// <summary>
    /// Sets whether duplicate category values are allowed. When false, duplicate categories will be merged.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="allowDuplicatedCategory">True to allow duplicates, false to merge them.</param>
    /// <returns>A new axis instance with the updated duplicate category setting.</returns>
    public static T AllowDuplicatedCategory<T>(this T axis, bool allowDuplicatedCategory) where T : AxisBase<T>
    {
        return axis with { AllowDuplicatedCategory = allowDuplicatedCategory };
    }

    /// <summary>
    /// Sets whether data points that exceed the axis domain are allowed to overflow beyond the chart boundaries.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="allowDataOverflow">True to allow data overflow, false to clip data to the axis domain.</param>
    /// <returns>A new axis instance with the updated data overflow setting.</returns>
    public static T AllowDataOverflow<T>(this T axis, bool allowDataOverflow) where T : AxisBase<T>
    {
        return axis with { AllowDataOverflow = allowDataOverflow };
    }

    /// <summary>
    /// Sets the rotation angle of the axis labels in degrees. Positive values rotate clockwise.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="angle">The rotation angle in degrees.</param>
    /// <returns>A new axis instance with the updated label rotation angle.</returns>
    public static T Angle<T>(this T axis, double angle) where T : AxisBase<T>
    {
        return axis with { Angle = angle };
    }

    /// <summary>
    /// Sets the number of tick marks to display on the axis. The actual number may be adjusted for optimal spacing.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="tickCount">The desired number of tick marks.</param>
    /// <returns>A new axis instance with the updated tick count.</returns>
    public static T TickCount<T>(this T axis, int tickCount) where T : AxisBase<T>
    {
        return axis with { TickCount = tickCount };
    }

    /// <summary>
    /// Sets whether hidden data points are included in axis calculations. When true, hidden data affects the axis range.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="includeHidden">True to include hidden data in calculations, false to exclude it.</param>
    /// <returns>A new axis instance with the updated hidden data inclusion setting.</returns>
    public static T IncludeHidden<T>(this T axis, bool includeHidden) where T : AxisBase<T>
    {
        return axis with { IncludeHidden = includeHidden };
    }

    /// <summary>
    /// Sets the display name for the axis, typically shown in tooltips or legends.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="name">The display name for the axis.</param>
    /// <returns>A new axis instance with the updated name.</returns>
    public static T Name<T>(this T axis, string name) where T : AxisBase<T>
    {
        return axis with { Name = name };
    }

    /// <summary>
    /// Sets the unit of measurement for the axis values (e.g., "px", "kg", "%", "$").
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="unit">The unit of measurement.</param>
    /// <returns>A new axis instance with the updated unit.</returns>
    public static T Unit<T>(this T axis, string unit) where T : AxisBase<T>
    {
        return axis with { Unit = unit };
    }

    /// <summary>
    /// Sets the label text displayed on the axis. This is separate from the Name property and is shown directly on the axis.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="label">The label text to display on the axis.</param>
    /// <returns>A new axis instance with the updated label.</returns>
    public static T Label<T>(this T axis, string label) where T : AxisBase<T>
    {
        return axis with { Label = label };
    }

    /// <summary>
    /// Sets whether the axis values are displayed in reverse order. When true, the highest value appears at the start.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="reversed">True to reverse the axis order, false for normal order.</param>
    /// <returns>A new axis instance with the updated reversal setting.</returns>
    public static T Reversed<T>(this T axis, bool reversed = true) where T : AxisBase<T>
    {
        return axis with { Reversed = reversed };
    }

    /// <summary>
    /// Sets whether the axis is mirrored across the chart center. Useful for creating symmetric charts.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="mirror">True to mirror the axis, false for normal positioning.</param>
    /// <returns>A new axis instance with the updated mirror setting.</returns>
    public static T Mirror<T>(this T axis, bool mirror = true) where T : AxisBase<T>
    {
        return axis with { Mirror = mirror };
    }

    /// <summary>
    /// Sets the scale transformation applied to the axis data. Different scales are used to transform data values
    /// for appropriate visualization based on the data characteristics.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="scale">The scale type to apply to the axis data.</param>
    /// <returns>A new axis instance with the updated scale.</returns>
    public static T Scale<T>(this T axis, AxisScales scale) where T : AxisBase<T>
    {
        return axis with { Scale = scale };
    }

    /// <summary>
    /// Sets the size of tick marks in pixels. Tick marks are the small lines that extend from the axis to help with value reading.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="tickSize">The size of tick marks in pixels.</param>
    /// <returns>A new axis instance with the updated tick size.</returns>
    public static T TickSize<T>(this T axis, int tickSize) where T : AxisBase<T>
    {
        return axis with { TickSize = tickSize };
    }

    /// <summary>
    /// Sets both the starting and ending values for the axis domain. Use "auto" for automatic calculation or specify specific values.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="start">The starting value for the axis domain.</param>
    /// <param name="end">The ending value for the axis domain.</param>
    /// <returns>A new axis instance with the updated domain range.</returns>
    public static T Domain<T>(this T axis, object start, object end) where T : AxisBase<T>
    {
        return axis with { DomainStart = start, DomainEnd = end };
    }

    /// <summary>
    /// Sets whether tick lines are displayed. Tick lines are the small lines that extend from the axis to help with value reading.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="tickLine">True to show tick lines, false to hide them.</param>
    /// <returns>A new axis instance with the updated tick line visibility.</returns>
    public static T TickLine<T>(this T axis, bool tickLine = true) where T : AxisBase<T>
    {
        return axis with { TickLine = tickLine };
    }

    /// <summary>
    /// Sets whether the main axis line is displayed. The axis line is the primary line that represents the axis.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="axisLine">True to show the axis line, false to hide it.</param>
    /// <returns>A new axis instance with the updated axis line visibility.</returns>
    public static T AxisLine<T>(this T axis, bool axisLine = true) where T : AxisBase<T>
    {
        return axis with { AxisLine = axisLine };
    }

    /// <summary>
    /// Sets the height of the X-axis in pixels. This affects the space allocated for axis labels and tick marks.
    /// </summary>
    /// <param name="axis">The X-axis to configure.</param>
    /// <param name="height">The height of the X-axis in pixels.</param>
    /// <returns>A new XAxis instance with the updated height.</returns>
    public static XAxis Height(this XAxis axis, int height)
    {
        return axis with { Height = height };
    }

    /// <summary>
    /// Sets the width of the Y-axis in pixels. This affects the space allocated for axis labels and tick marks.
    /// </summary>
    /// <param name="axis">The Y-axis to configure.</param>
    /// <param name="width">The width of the Y-axis in pixels.</param>
    /// <returns>A new YAxis instance with the updated width.</returns>
    public static YAxis Width(this YAxis axis, int width)
    {
        return axis with { Width = width };
    }

    /// <summary>
    /// Sets the minimum gap between tick marks in pixels. This ensures tick labels don't overlap.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="minTickGap">The minimum gap between tick marks in pixels.</param>
    /// <returns>A new axis instance with the updated minimum tick gap.</returns>
    public static T MinTickGap<T>(this T axis, int minTickGap) where T : AxisBase<T>
    {
        return axis with { MinTickGap = minTickGap };
    }

    /// <summary>
    /// Sets whether the axis is completely hidden from view. When true, the axis and all its elements are invisible.
    /// </summary>
    /// <typeparam name="T">The axis type that inherits from AxisBase.</typeparam>
    /// <param name="axis">The axis to configure.</param>
    /// <param name="hide">True to hide the axis, false to show it.</param>
    /// <returns>A new axis instance with the updated visibility setting.</returns>
    public static T Hide<T>(this T axis, bool hide = true) where T : AxisBase<T>
    {
        return axis with { Hide = hide };
    }
}