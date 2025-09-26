using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A loading placeholder widget that creates animated shimmer effects to mimic content structure.
/// </summary>
/// <remarks>
/// Creates animated placeholder blocks that improve perceived performance by showing users 
/// the layout structure while actual content is loading. Commonly used with custom width and height.
/// </remarks>
public record Skeleton : WidgetBase<Skeleton>
{
    /// <summary>
    /// Initializes a new skeleton placeholder with full width and height.
    /// </summary>
    public Skeleton()
    {
        Width = Size.Full();
        Height = Size.Full();
    }
}