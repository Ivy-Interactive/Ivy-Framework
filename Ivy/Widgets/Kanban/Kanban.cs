using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Kanban widget displaying structured data in kanban board format with <see cref="KanbanColumn"/> elements supporting pipe operator for easy column addition.</summary>
public record Kanban : WidgetBase<Kanban>
{
    /// <param name="columns">KanbanColumn elements defining kanban board structure and content.</param>
    public Kanban(params KanbanColumn[] columns) : base(columns.Cast<object>().ToArray())
    {
    }

    [Prop] public bool ShowCounts { get; set; } = true;

    [Prop] public bool AllowAdd { get; set; }

    [Prop] public bool AllowMove { get; set; }

    [Prop] public bool AllowDelete { get; set; }

    [Event] public Func<Event<Kanban, object?>, ValueTask>? OnDelete { get; set; }

    [Event] public Func<Event<Kanban, (object? CardId, object? FromColumn, object? ToColumn, int? TargetIndex)>, ValueTask>? OnMove { get; set; }

    /// <param name="kanban">Kanban to add column to.</param>
    /// <param name="child">KanbanColumn to add to kanban board.</param>
    /// <returns>New Kanban instance with additional column appended.</returns>
    public static Kanban operator |(Kanban kanban, KanbanColumn child)
    {
        return kanban with { Children = [.. kanban.Children, child] };
    }
}
