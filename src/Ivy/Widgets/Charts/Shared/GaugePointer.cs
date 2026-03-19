
// ReSharper disable once CheckNamespace
namespace Ivy;

public record GaugeThreshold(double Value, string Color);

public enum GaugePointerStyle
{
    Line,
    Arrow,
    Rounded
}

public record GaugePointer
{
    public GaugePointer()
    {
    }

    public GaugePointerStyle Style { get; set; } = GaugePointerStyle.Arrow;

    public int Width { get; set; } = 6;

    public string Length { get; set; } = "60%";
}

public static class GaugePointerExtensions
{
    public static GaugePointer Style(this GaugePointer pointer, GaugePointerStyle style)
    {
        return pointer with { Style = style };
    }

    public static GaugePointer Width(this GaugePointer pointer, int width)
    {
        return pointer with { Width = width };
    }

    public static GaugePointer Length(this GaugePointer pointer, string length)
    {
        return pointer with { Length = length };
    }
}
