
// ReSharper disable once CheckNamespace
namespace Ivy;

public record PolarRadiusAxis
{
    [Prop] public double? Angle { get; init; } = null;
    [Prop] public object[]? Domain { get; init; } = null;
    [Prop] public int? TickCount { get; init; } = null;
    [Prop] public Colors? Stroke { get; init; } = null;
}
