using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A visual board for managing tasks and workflows.
/// </summary>
public record Kanban : WidgetBase<Kanban>
{
    public Kanban(params KanbanCard[] cards) : base([.. cards])
    {
    }

    internal Kanban() { }

    [Prop] public bool ShowCounts { get; set; } = true;

    [Prop] public Size? ColumnWidth { get; set; }

    [Event] public EventHandler<Event<Kanban, (object? CardId, object? ToColumn, int? TargetIndex)>>? OnCardMove { get; set; }

    public static Kanban operator |(Kanban kanban, KanbanCard child)
    {
        return kanban with { Children = [.. kanban.Children, child] };
    }
}
