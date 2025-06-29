

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public record ReferenceDot
{
    public ReferenceDot(double x, double y, string? label = null)
    {
        X = x;
        Y = y;
        Label = label;
    }

    public double X { get; set; }
    public double Y { get; set; }
    public string? Label { get; set; }
}

public static class ReferenceDotExtensions
{
    public static ReferenceDot Label(this ReferenceDot referenceDot, string label)
    {
        return referenceDot with { Label = label };
    }
}