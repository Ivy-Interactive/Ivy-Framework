using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views.Kanban;

public class KanbanView<TModel, TGroupKey>(IEnumerable<TModel> model, Func<TModel, TGroupKey> groupBySelector) : ViewBase, IStateless
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
