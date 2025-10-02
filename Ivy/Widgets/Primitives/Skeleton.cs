using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Animated loading placeholder with shimmer effect. Default size: full width and height.</summary>
public record Skeleton : WidgetBase<Skeleton>
{
    /// <summary>Initializes skeleton placeholder.</summary>
    public Skeleton()
    {
        Width = Size.Full();
        Height = Size.Full();
    }
}