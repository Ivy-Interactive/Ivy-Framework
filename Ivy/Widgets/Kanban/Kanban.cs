using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Kanban widget displaying structured data in kanban board format with <see cref="KanbanColumn"/> elements supporting pipe operator for easy column addition.</summary>
public record Kanban : WidgetBase<Kanban>
{
    /// <summary>Initializes Kanban with specified kanban columns.</summary>
    /// <param name="columns">KanbanColumn elements defining kanban board structure and content.</param>
    public Kanban(params KanbanColumn[] columns) : base(columns.Cast<object>().ToArray())
    {
    }

    /// <summary>Whether to show card counts in column headers. Default is true.</summary>
    [Prop] public bool ShowCounts { get; set; } = true;

    /// <summary>Whether to allow adding cards to columns. Automatically set to true when HandleAdd is configured.</summary>
    [Prop] public bool AllowAdd { get; set; }

    /// <summary>Whether to allow moving cards between columns. Automatically set to true when HandleMove is configured.</summary>
    [Prop] public bool AllowMove { get; set; }

    /// <summary>Whether to allow deleting cards. Automatically set to true when HandleDelete is configured.</summary>
    [Prop] public bool AllowDelete { get; set; }

    /// <summary>Event handler called when a card is deleted from the kanban board.</summary>
    [Event] public Func<Event<Kanban, object?>, ValueTask>? OnDelete { get; set; }

    /// <summary>Event handler called when a card is moved between columns.</summary>
    [Event] public Func<Event<Kanban, (object? CardId, object? FromColumn, object? ToColumn)>, ValueTask>? OnMove { get; set; }

    /// <summary>Allows adding single KanbanColumn using pipe operator for convenient kanban board construction.</summary>
    /// <param name="kanban">Kanban to add column to.</param>
    /// <param name="child">KanbanColumn to add to kanban board.</param>
    /// <returns>New Kanban instance with additional column appended.</returns>
    public static Kanban operator |(Kanban kanban, KanbanColumn child)
    {
        return kanban with { Children = [.. kanban.Children, child] };
    }
}
