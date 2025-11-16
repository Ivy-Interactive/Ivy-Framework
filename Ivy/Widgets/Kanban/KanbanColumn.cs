using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record KanbanColumn : WidgetBase<KanbanColumn>
{
    public KanbanColumn(params KanbanCard[] cards) : base(cards.Cast<object>().ToArray())
    {
    }

    [Prop] public string? Title { get; set; }

    [Prop] public object? ColumnKey { get; set; }

    [Event] public Func<Event<KanbanColumn, object?>, ValueTask>? OnAdd { get; set; }

    public static KanbanColumn operator |(KanbanColumn column, KanbanCard child)
    {
        return column with { Children = [.. column.Children, child] };
    }
}

public static class KanbanColumnExtensions
{
    public static KanbanColumn Title(this KanbanColumn column, string title)
    {
        return column with { Title = title };
    }

    public static KanbanColumn ColumnKey(this KanbanColumn column, object? columnKey)
    {
        return column with { ColumnKey = columnKey };
    }
}
