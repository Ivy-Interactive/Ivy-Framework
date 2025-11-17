using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Kanban widget displaying structured data in kanban board format with <see cref="KanbanCard"/> elements supporting pipe operator for easy card addition.</summary>
public record Kanban : WidgetBase<Kanban>
{
    public Kanban(params KanbanCard[] cards) : base([.. cards.Cast<object>()])
    {
    }

    [Prop] public bool ShowCounts { get; set; } = true;

    [Prop] public bool AllowAdd { get; set; }

    [Prop] public bool AllowMove { get; set; }

    [Prop] public bool AllowDelete { get; set; }

    [Event] public Func<Event<Kanban, object?>, ValueTask>? OnDelete { get; set; }

    [Event] public Func<Event<Kanban, (object? CardId, object? ToColumn, int? TargetIndex)>, ValueTask>? OnCardMove { get; set; }

    public static Kanban operator |(Kanban kanban, KanbanCard child)
    {
        return kanban with { Children = [.. kanban.Children, child] };
    }
}
