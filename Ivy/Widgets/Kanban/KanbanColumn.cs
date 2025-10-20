using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Kanban column widget containing a collection of kanban cards with optional title and pipe operator for card addition.</summary>
public record KanbanColumn : WidgetBase<KanbanColumn>
{
    /// <summary>Initializes KanbanColumn with specified kanban cards.</summary>
    /// <param name="cards">KanbanCard elements defining column's content.</param>
    public KanbanColumn(params KanbanCard[] cards) : base(cards.Cast<object>().ToArray())
    {
    }

    /// <summary>Optional title for the kanban column. When set, displayed as header above cards.</summary>
    [Prop] public string? Title { get; set; }

    /// <summary>Optional column key value used to identify the column in event handlers.</summary>
    [Prop] public object? ColumnKey { get; set; }

    /// <summary>Event handler called when a card is added to this column.</summary>
    [Event] public Func<Event<KanbanColumn, object?>, ValueTask>? OnAdd { get; set; }

    /// <summary>Allows adding single KanbanCard using pipe operator for convenient column construction.</summary>
    /// <param name="column">KanbanColumn to add card to.</param>
    /// <param name="child">KanbanCard to add to column.</param>
    /// <returns>New KanbanColumn instance with additional card appended.</returns>
    public static KanbanColumn operator |(KanbanColumn column, KanbanCard child)
    {
        return column with { Children = [.. column.Children, child] };
    }
}

/// <summary>Extension methods for KanbanColumn widget providing fluent API for configuring column properties.</summary>
public static class KanbanColumnExtensions
{
    /// <summary>Sets the title for kanban column.</summary>
    /// <param name="column">KanbanColumn to configure.</param>
    /// <param name="title">Title text to display as column header.</param>
    /// <returns>New KanbanColumn instance with updated title.</returns>
    public static KanbanColumn Title(this KanbanColumn column, string title)
    {
        return column with { Title = title };
    }

    /// <summary>Sets the column key for kanban column.</summary>
    /// <param name="column">KanbanColumn to configure.</param>
    /// <param name="columnKey">Key value to identify the column.</param>
    /// <returns>New KanbanColumn instance with updated column key.</returns>
    public static KanbanColumn ColumnKey(this KanbanColumn column, object? columnKey)
    {
        return column with { ColumnKey = columnKey };
    }
}
