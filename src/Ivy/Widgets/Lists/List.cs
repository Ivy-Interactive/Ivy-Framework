// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Displays a sequence of items in a list format.
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