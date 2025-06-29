

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public record ReferenceArea
{
    public ReferenceArea(double x1, double y1, double x2, double y2, string? label = null)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
        Label = label;
    }

    public double X1 { get; set; }
    public double Y1 { get; set; }
    public double X2 { get; set; }
    public double Y2 { get; set; }
    public string? Label { get; set; }

}

public static class ReferenceAreaExtensions
{
    public static ReferenceArea Label(this ReferenceArea referenceArea, string label)
    {
        return referenceArea with { Label = label };
    }
}