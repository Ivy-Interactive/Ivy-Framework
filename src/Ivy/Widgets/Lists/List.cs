using Ivy.Core;

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

    /// <summary>
    /// Enables drag-and-drop reordering of list items.
    /// </summary>
    [Prop] public bool Reorderable { get; set; }

    /// <summary>
    /// Event fired when items are reordered via drag-and-drop.
    /// The event value contains the ordered list of item IDs.
    /// </summary>
    [Event] public EventHandler<Event<List, string[]>>? OnReorder { get; set; }
}