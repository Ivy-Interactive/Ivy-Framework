using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Kanban widget displaying structured data in kanban board format with <see cref="KanbanColumn"/> elements supporting pipe operator for easy column addition.</summary>
public record Kanban : WidgetBase<Kanban>
{
    public Kanban(params KanbanColumn[] columns) : base(columns.Cast<object>().ToArray())
    {
    }

    [Prop] public bool ShowCounts { get; set; } = true;

    [Prop] public bool AllowAdd { get; set; }

    [Prop] public bool AllowMove { get; set; }

    [Prop] public bool AllowDelete { get; set; }

    [Event] public Func<Event<Kanban, object?>, ValueTask>? OnDelete { get; set; }

    [Event] public Func<Event<Kanban, (object? CardId, object? FromColumn, object? ToColumn, int? TargetIndex)>, ValueTask>? OnMove { get; set; }

    public static Kanban operator |(Kanban kanban, KanbanColumn child)
    {
        return kanban with { Children = [.. kanban.Children, child] };
    }
}
