using Ivy.Charts;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A hierarchical visualization plotted in polar coordinates - essentially a treemap rendered as concentric rings.
/// The innermost ring represents the root level, with each subsequent ring representing deeper levels.
/// </summary>
public record SunburstChart : WidgetBase<SunburstChart>
{
  public SunburstChart(object data) : this()
  {
    Data = data;
  }

  internal SunburstChart()
  {
    Width = Size.Full();
    Height = Size.Full();
  }

  /// <summary>
  /// Hierarchical data for the sunburst chart.
  /// Can be an array of SunburstNode objects.
  /// </summary>
  [Prop] public object? Data { get; init; }

  [Prop] public string DataKey { get; init; } = "value";

  [Prop] public string NameKey { get; init; } = "name";

  [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;

  [Prop] public Charts.Tooltip? Tooltip { get; init; }

  /// <summary>
  /// Inner radius of the sunburst chart, in pixels or percentage. Controls the size of the hole.
  /// </summary>
  [Prop] public string? InnerRadius { get; init; }

  /// <summary>
  /// Outer radius of the sunburst chart, in pixels or percentage. When null, it fills the available space.
  /// </summary>
  [Prop] public string? OuterRadius { get; init; }

  /// <summary>
  /// X coordinate of the center. When null, defaults to "50%".
  /// </summary>
  [Prop] public string? Cx { get; init; }

  /// <summary>
  /// Y coordinate of the center. When null, defaults to "50%".
  /// </summary>
  [Prop] public string? Cy { get; init; }

  /// <summary>
  /// Start angle of the sunburst chart.
  /// </summary>
  [Prop] public int StartAngle { get; init; } = 90;

  /// <summary>
  /// End angle of the sunburst chart. Default doesn't need to be set since total is based on 360 relative to start.
  /// </summary>
  [Prop] public int? EndAngle { get; init; }

  /// <summary>
  /// Padding adds spacing between individual sectors within the same ring.
  /// </summary>
  [Prop] public int Padding { get; init; } = 2;

  /// <summary>
  /// RingPadding adds spacing between concentric rings (hierarchical levels).
  /// </summary>
  [Prop] public int RingPadding { get; init; } = 2;

  /// <summary>
  /// Color of the borders between segments. Defaults to white.
  /// </summary>
  [Prop] public Colors Stroke { get; init; } = Colors.White;

  public static SunburstChart operator |(SunburstChart widget, object child)
  {
    throw new NotSupportedException("SunburstChart does not support children.");
  }
}

public static class SunburstChartExtensions
{
  public static SunburstChart ColorScheme(this SunburstChart chart, ColorScheme colorScheme)
  {
    return chart with { ColorScheme = colorScheme };
  }

  public static SunburstChart Tooltip(this SunburstChart chart, Charts.Tooltip tooltip)
  {
    return chart with { Tooltip = tooltip };
  }

  public static SunburstChart Tooltip(this SunburstChart chart)
  {
    return chart with { Tooltip = new Charts.Tooltip() };
  }

  public static SunburstChart InnerRadius(this SunburstChart chart, int value)
  {
    return chart with { InnerRadius = $"{value}px" };
  }

  public static SunburstChart InnerRadius(this SunburstChart chart, string value)
  {
    return chart with { InnerRadius = value };
  }

  public static SunburstChart OuterRadius(this SunburstChart chart, int value)
  {
    return chart with { OuterRadius = $"{value}px" };
  }

  public static SunburstChart OuterRadius(this SunburstChart chart, string value)
  {
    return chart with { OuterRadius = value };
  }

  public static SunburstChart Cx(this SunburstChart chart, int value)
  {
    return chart with { Cx = $"{value}px" };
  }

  public static SunburstChart Cx(this SunburstChart chart, string value)
  {
    return chart with { Cx = value };
  }

  public static SunburstChart Cy(this SunburstChart chart, int value)
  {
    return chart with { Cy = $"{value}px" };
  }

  public static SunburstChart Cy(this SunburstChart chart, string value)
  {
    return chart with { Cy = value };
  }

  public static SunburstChart StartAngle(this SunburstChart chart, int value)
  {
    return chart with { StartAngle = value };
  }

  public static SunburstChart EndAngle(this SunburstChart chart, int value)
  {
    return chart with { EndAngle = value };
  }

  public static SunburstChart Padding(this SunburstChart chart, int value)
  {
    return chart with { Padding = value };
  }

  public static SunburstChart RingPadding(this SunburstChart chart, int value)
  {
    return chart with { RingPadding = value };
  }

  public static SunburstChart Stroke(this SunburstChart chart, Colors value)
  {
    return chart with { Stroke = value };
  }
}
