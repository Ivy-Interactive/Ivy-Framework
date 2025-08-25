

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Represents a tooltip configuration.
/// </summary>
public record Tooltip
{
    /// <summary>
    /// Initializes a new instance of the Tooltip class.
    /// </summary>
    public Tooltip()
    {

    }

    /// <summary>
    /// Gets or sets whether the tooltip should be animated.
    /// </summary>
    public bool Animated { get; set; } = false;

}

/// <summary>
/// Extension methods for the Tooltip class.
/// </summary>
public static class TooltipExtensions
{
    /// <summary>
    /// Sets whether the tooltip should be animated.
    /// </summary>
    /// <param name="tooltip">The Tooltip to configure.</param>
    /// <param name="animated">True to enable animation, false to disable.</param>
    /// <returns>A new Tooltip instance with the updated animation setting.</returns>
    public static Tooltip Animated(this Tooltip tooltip, bool animated)
    {
        return tooltip with { Animated = animated };
    }
}