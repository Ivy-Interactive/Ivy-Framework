using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record KanbanCard : WidgetBase<KanbanCard>
{
    public KanbanCard(object? content) : base(content != null ? [content] : [])
    {
    }

    [Prop] public object? CardId { get; set; }

    [Prop] public object? Priority { get; set; }

    [Event] public Func<Event<KanbanCard, object?>, ValueTask>? OnClick { get; set; }
}
