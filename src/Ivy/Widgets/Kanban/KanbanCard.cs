using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A single card item within a Kanban board.
/// </summary>
public record KanbanCard : WidgetBase<KanbanCard>
{
    public KanbanCard(object? content) : base(content != null ? [content] : [])
    {
    }

    internal KanbanCard() { }

    [Prop] public object? CardId { get; set; }

    [Prop] public object? Priority { get; set; }

    [Prop] public object? Column { get; set; }

    [Prop] public string? ColumnName { get; set; }
}
