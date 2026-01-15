using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views.Kanban;

/// <summary>
/// A view for rendering a Kanban board from a collection of grouped models.
/// Displays cards in columns based on group keys.
/// </summary>
public class KanbanView<TModel, TGroupKey>(IEnumerable<TModel> model) : ViewBase, IStateless
    where TGroupKey : notnull
{
    public override object? Build()
    {
        var cards = model.Select(item => new KanbanCard(item)).ToArray();

        return new Ivy.Kanban(cards) with
        {
            Width = Size.Full(),
            Height = Size.Full()
        };
    }
}
