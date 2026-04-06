// ReSharper disable once CheckNamespace
namespace Ivy;

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
