

// ReSharper disable once CheckNamespace
namespace Ivy;

public record ChartTooltip
{
    public ChartTooltip()
    {

    }

    public bool Animated { get; set; } = false;
    public string? ValueFormat { get; set; } = null;
    public TickFormatterType ValueFormatType { get; set; } = TickFormatterType.Auto;
}

public static class ChartTooltipExtensions
{
    public static ChartTooltip Animated(this ChartTooltip tooltip, bool animated)
    {
        return tooltip with { Animated = animated };
    }

    public static ChartTooltip ValueFormat(this ChartTooltip tooltip, string format)
    {
        return tooltip with { ValueFormat = format };
    }

    public static ChartTooltip ValueFormat(this ChartTooltip tooltip, string format, TickFormatterType type)
    {
        return tooltip with { ValueFormat = format, ValueFormatType = type };
    }
}