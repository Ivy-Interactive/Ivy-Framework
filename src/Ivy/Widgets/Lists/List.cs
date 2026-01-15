using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A simple container for displaying a vertical list of items.
/// </summary>
public record List : WidgetBase<List>
{
    public List(params object[] items) : base(items)
    {
    }

    public List(IEnumerable<object> items) : base(items.ToArray())
    {
    }

    internal List()
    {
    }
}