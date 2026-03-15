
// ReSharper disable once CheckNamespace
namespace Ivy;

public record ZAxis
{
    public ZAxis(string? dataKey = null)
    {
        DataKey = dataKey;
    }

    internal ZAxis()
    {
    }

    public string? DataKey { get; init; }

    public int RangeMin { get; set; } = 60;

    public int RangeMax { get; set; } = 400;

    public string? Unit { get; set; } = null;

    public string? Name { get; set; } = null;

    public AxisScales Scale { get; set; } = AxisScales.Auto;
}

public static class ZAxisExtensions
{
    public static ZAxis Range(this ZAxis axis, int min, int max)
    {
        return axis with { RangeMin = min, RangeMax = max };
    }

    public static ZAxis Unit(this ZAxis axis, string unit)
    {
        return axis with { Unit = unit };
    }

    public static ZAxis Name(this ZAxis axis, string name)
    {
        return axis with { Name = name };
    }

    public static ZAxis Scale(this ZAxis axis, AxisScales scale)
    {
        return axis with { Scale = scale };
    }
}
