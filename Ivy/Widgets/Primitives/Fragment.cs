using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Fragment : WidgetBase<Fragment>
{
    public Fragment(params object?[] children) : base(children.Where(e => e != null).ToArray()!)
    {
    }
}