using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views.Kanban;

/// <summary>
/// Renders a kanban board from data models by grouping items into columns based on a selector function.
/// </summary>
/// <typeparam name="TModel">The type of data objects to display in kanban cards.</typeparam>
/// <typeparam name="TGroupKey">The type of the grouping key used to organize items into columns.</typeparam>
/// <param name="model">The collection of data objects to render as kanban cards.</param>
/// <param name="groupBySelector">Function that determines which column each item belongs to.</param>
public class KanbanView<TModel, TGroupKey>(IEnumerable<TModel> model, Func<TModel, TGroupKey> groupBySelector) : ViewBase, IStateless
    where TGroupKey : notnull
{
    /// <summary>
    /// Builds the kanban board by grouping items and creating columns with cards.
    /// </summary>
    /// <returns>A Kanban widget containing all columns and their cards.</returns>
    public override object? Build()
    {
        var grouped = model.GroupBy(groupBySelector);

        var columns = grouped.Select(group =>
        {
            var cards = group.Select(item => new KanbanCard(item)).ToArray();
            return new KanbanColumn(cards).Title(group.Key?.ToString() ?? "");
        }).ToArray();

        return new Ivy.Kanban(columns) with
        {
            Width = Size.Full(),
            Height = Size.Full()
        };
    }
}
