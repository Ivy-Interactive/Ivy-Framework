using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>A container widget that displays a collection of items in a vertical list layout. Provides a simple and flexible way to render multiple widgets or data items in an organized list format with consistent spacing and styling.</summary>
public record List : WidgetBase<List>
{
    /// <param name="items">The items to display in the list. Can be widgets, strings, or any objects that can be rendered.</param>
    public List(params object[] items) : base(items)
    {
    }

    /// <param name="items">The enumerable collection of items to display in the list.</param>
    public List(IEnumerable<object> items) : base(items.ToArray())
    {
    }
}