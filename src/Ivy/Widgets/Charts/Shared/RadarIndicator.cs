
// ReSharper disable once CheckNamespace
namespace Ivy;

public record RadarIndicator
{
    public RadarIndicator(string name)
    {
        Name = name;
    }

    internal RadarIndicator()
    {
    }

    [Prop] public string? Name { get; }
    [Prop] public double? Max { get; set; }
    [Prop] public double? Min { get; set; } = 0;
}

public static class RadarIndicatorExtensions
{
    public static RadarIndicator Max(this RadarIndicator indicator, double max)
        => indicator with { Max = max };

    public static RadarIndicator Min(this RadarIndicator indicator, double min)
        => indicator with { Min = min };
}
