

// ReSharper disable once CheckNamespace
namespace Ivy;

public record ChartTooltip
{
    public ChartTooltip()
    {

    }

    public bool Animated { get; set; } = false;
}

public static class ChartTooltipExtensions
{
    public static ChartTooltip Animated(this ChartTooltip tooltip, bool animated)
    {
        return tooltip with { Animated = animated };
    }
}