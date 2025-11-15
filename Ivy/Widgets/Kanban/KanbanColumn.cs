using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record KanbanColumn : WidgetBase<KanbanColumn>
{
    /// <param name="cards">KanbanCard elements defining column's content.</param>
    public KanbanColumn(params KanbanCard[] cards) : base(cards.Cast<object>().ToArray())
    {
    }

    [Prop] public string? Title { get; set; }

    [Prop] public object? ColumnKey { get; set; }

    [Event] public Func<Event<KanbanColumn, object?>, ValueTask>? OnAdd { get; set; }

    /// <param name="column">KanbanColumn to add card to.</param>
    /// <param name="child">KanbanCard to add to column.</param>
    /// <returns>New KanbanColumn instance with additional card appended.</returns>
    public static KanbanColumn operator |(KanbanColumn column, KanbanCard child)
    {
        return column with { Children = [.. column.Children, child] };
    }
}

public static class KanbanColumnExtensions
{
    /// <param name="column">KanbanColumn to configure.</param>
    /// <param name="title">Title text to display as column header.</param>
    /// <returns>New KanbanColumn instance with updated title.</returns>
    public static KanbanColumn Title(this KanbanColumn column, string title)
    {
        return column with { Title = title };
    }

    /// <param name="column">KanbanColumn to configure.</param>
    /// <param name="columnKey">Key value to identify the column.</param>
    /// <returns>New KanbanColumn instance with updated column key.</returns>
    public static KanbanColumn ColumnKey(this KanbanColumn column, object? columnKey)
    {
        return column with { ColumnKey = columnKey };
    }
}
