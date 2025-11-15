using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Kanban card widget representing an individual item within a kanban column.</summary>
public record KanbanCard : WidgetBase<KanbanCard>
{
    /// <param name="content">Content to display within kanban card.</param>
    public KanbanCard(object? content) : base(content != null ? [content] : [])
    {
    }

    [Prop] public object? CardId { get; set; }

    [Prop] public object? Priority { get; set; }

    [Event] public Func<Event<KanbanCard, object?>, ValueTask>? OnClick { get; set; }
}
