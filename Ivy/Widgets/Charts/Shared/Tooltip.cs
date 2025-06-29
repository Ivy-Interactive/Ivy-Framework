

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public record Tooltip
{
    public Tooltip()
    {

    }

    public bool Animated { get; set; } = false;

}

public static class TooltipExtensions
{
    public static Tooltip Animated(this Tooltip tooltip, bool animated)
    {
        return tooltip with { Animated = animated };
    }
}