using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views.Kanban;

/// <summary>
/// Renders a kanban board from data models as kanban cards.
/// </summary>
/// <typeparam name="TModel">The type of data objects to display in kanban cards.</typeparam>
/// <typeparam name="TGroupKey">The type of the grouping key (kept for compatibility, not currently used).</typeparam>
/// <param name="model">The collection of data objects to render as kanban cards.</param>
/// <param name="groupBySelector">Function that determines grouping (kept for compatibility, not currently used).</param>
public class KanbanView<TModel, TGroupKey>(IEnumerable<TModel> model, Func<TModel, TGroupKey> groupBySelector) : ViewBase, IStateless
    where TGroupKey : notnull
{
    /// <summary>
    /// Builds the kanban board with cards from all items.
    /// </summary>
    /// <returns>A Kanban widget containing all cards.</returns>
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
