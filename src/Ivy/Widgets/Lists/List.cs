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

    [Prop] public bool Reorderable { get; set; }
    [Event] public EventHandler<Event<List, string[]>>? OnReorder { get; set; }
}

public static class ListExtensions
{
    public static List Reorderable(this List list, bool reorderable = true) => list with { Reorderable = reorderable };
    public static List OnReorder(this List list, EventHandler<Event<List, string[]>> onReorder) => list with { OnReorder = onReorder };
    public static List OnReorder(this List list, Action<Event<List, string[]>> onReorder) => list with { OnReorder = onReorder.ToEventHandler() };
}