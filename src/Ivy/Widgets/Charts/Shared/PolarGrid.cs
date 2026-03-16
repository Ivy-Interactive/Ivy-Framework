
// ReSharper disable once CheckNamespace
namespace Ivy;

public record PolarGrid
{
    [Prop] public PolarGridTypes GridType { get; init; } = PolarGridTypes.Polygon;
    [Prop] public Colors? Stroke { get; init; } = null;
    [Prop] public bool RadialLines { get; init; } = true;
}

public enum PolarGridTypes
{
    Polygon,
    Circle
}

public static class PolarGridExtensions
{
    public static PolarGrid GridType(this PolarGrid grid, PolarGridTypes gridType)
        => grid with { GridType = gridType };

    public static PolarGrid Stroke(this PolarGrid grid, Colors stroke)
        => grid with { Stroke = stroke };

    public static PolarGrid RadialLines(this PolarGrid grid, bool radialLines = true)
        => grid with { RadialLines = radialLines };
}
