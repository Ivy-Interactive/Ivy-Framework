using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record List : WidgetBase<List>
{
    public List(params object[] items) : base(items)
    {
    }

    public List(IEnumerable<object> items) : base(items.ToArray())
    {
    }
}