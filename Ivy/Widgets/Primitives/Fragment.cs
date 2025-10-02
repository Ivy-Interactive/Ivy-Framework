using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Transparent container grouping multiple widgets without adding layout structure. Filters out null children.</summary>
public record Fragment : WidgetBase<Fragment>
{
    /// <summary>Initializes fragment with child widgets (nulls filtered out).</summary>
    /// <param name="children">Child widgets to group.</param>
    public Fragment(params object?[] children) : base(children.Where(e => e != null).ToArray()!)
    {
    }
}