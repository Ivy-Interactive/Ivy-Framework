using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Kanban : WidgetBase<Kanban>
{
    public Kanban(params KanbanCard[] cards) : base([.. cards.Cast<object>()])
    {
    }

    [Prop] public bool ShowCounts { get; set; } = true;

    [Prop] public Size? ColumnWidth { get; set; }

    [Event] public Func<Event<Kanban, (object? CardId, object? ToColumn, int? TargetIndex)>, ValueTask>? OnCardMove { get; set; }

    public static Kanban operator |(Kanban kanban, KanbanCard child)
    {
        return kanban with { Children = [.. kanban.Children, child] };
    }
}
