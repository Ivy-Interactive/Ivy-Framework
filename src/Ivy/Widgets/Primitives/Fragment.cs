// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A logical container that renders its children without a wrapper element.
/// </summary>
public record Fragment : WidgetBase<Fragment>
{
    public Fragment(params object?[] children) : base(children.Where(e => e != null).ToArray()!)
    {
    }

    internal Fragment() { }
}