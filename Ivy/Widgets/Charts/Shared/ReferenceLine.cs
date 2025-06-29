

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public record ReferenceLine
{
    public ReferenceLine(double? x, double? y, string? label = null)
    {
        X = x;
        Y = y;
        Label = label;
    }

    public double? X { get; set; }
    public double? Y { get; set; }
    public string? Label { get; set; }
    public int StrokeWidth { get; set; } = 1;
}

public static class ReferenceLineExtensions
{
    public static ReferenceLine Label(this ReferenceLine referenceLine, string label)
    {
        return referenceLine with { Label = label };
    }

    public static ReferenceLine StrokeWidth(this ReferenceLine referenceLine, int strokeWidth)
    {
        return referenceLine with { StrokeWidth = strokeWidth };
    }
}