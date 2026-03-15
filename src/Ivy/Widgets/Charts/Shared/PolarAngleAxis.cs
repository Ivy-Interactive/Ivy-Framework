
// ReSharper disable once CheckNamespace
namespace Ivy;

public record PolarAngleAxis
{
    public PolarAngleAxis(string? dataKey = null)
    {
        DataKey = dataKey;
    }

    [Prop] public string? DataKey { get; init; }
    [Prop] public Colors? Stroke { get; init; } = null;
    [Prop] public bool AxisLine { get; init; } = true;
    [Prop] public bool TickLine { get; init; } = true;
}
