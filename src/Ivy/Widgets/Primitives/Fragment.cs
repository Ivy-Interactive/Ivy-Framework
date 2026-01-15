using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A logical wrapper that groups multiple children without adding an extra node to the DOM. Useful when a parent widget expects a single child but you need to return multiple.
/// </summary>
public record Fragment : WidgetBase<Fragment>
{
    public Fragment(params object?[] children) : base(children.Where(e => e != null).ToArray()!)
    {
    }

    internal Fragment() { }
}