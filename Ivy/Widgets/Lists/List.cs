using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A container widget that displays a collection of items in a vertical list layout.
/// Provides a simple and flexible way to render multiple widgets or data items
/// in an organized list format with consistent spacing and styling.
/// </summary>
public record List : WidgetBase<List>
{
    /// <summary>
    /// Initializes a new list with a variable number of items.
    /// Accepts any number of widgets, strings, or other objects as list items.
    /// </summary>
    /// <param name="items">The items to display in the list. Can be widgets, strings, or any objects that can be rendered.</param>
    /// <remarks>
    /// This constructor uses the params keyword to allow flexible item specification:
    /// <code>
    /// var list = new List(item1, item2, item3);
    /// </code>
    /// </remarks>
    public List(params object[] items) : base(items)
    {
    }

    /// <summary>
    /// Initializes a new list with items from an enumerable collection.
    /// Useful for creating lists from existing collections, LINQ queries, or dynamically generated content.
    /// </summary>
    /// <param name="items">The enumerable collection of items to display in the list.</param>
    /// <remarks>
    /// This constructor enables creation from any IEnumerable source:
    /// <code>
    /// var items = GetListItems();
    /// var list = new List(items);
    /// 
    /// // Or with LINQ
    /// var list = new List(data.Select(item => new ListItem(item.Name)));
    /// </code>
    /// </remarks>
    public List(IEnumerable<object> items) : base(items.ToArray())
    {
    }
}