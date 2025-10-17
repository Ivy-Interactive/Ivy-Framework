using System.Linq.Expressions;

namespace Ivy.Views.Kanban;

/// <summary>
/// Extension methods for converting collections to kanban builders with fluent configuration.
/// </summary>
public static class KanbanExtensions
{
    /// <summary>
    /// Converts any enumerable collection to a kanban builder with automatic column grouping.
    /// </summary>
    /// <typeparam name="TModel">The type of objects in the collection.</typeparam>
    /// <typeparam name="TGroupKey">The type of the grouping key used to organize items into columns.</typeparam>
    /// <param name="records">The collection to convert to a kanban board.</param>
    /// <param name="groupBySelector">Expression that determines which column each item belongs to.</param>
    /// <returns>A kanban builder for fluent configuration of columns, cards, and formatting.</returns>
    public static KanbanBuilder<TModel, TGroupKey> ToKanban<TModel, TGroupKey>(
        this IEnumerable<TModel> records,
        Expression<Func<TModel, TGroupKey>> groupBySelector)
        where TGroupKey : notnull
    {
        return new KanbanBuilder<TModel, TGroupKey>(records, groupBySelector.Compile());
    }

    /// <summary>
    /// Converts any enumerable collection to a kanban builder with automatic column grouping and card field selection.
    /// </summary>
    /// <typeparam name="TModel">The type of objects in the collection.</typeparam>
    /// <typeparam name="TGroupKey">The type of the grouping key used to organize items into columns.</typeparam>
    /// <param name="records">The collection to convert to a kanban board.</param>
    /// <param name="groupBySelector">Expression that determines which column each item belongs to.</param>
    /// <param name="cardIdSelector">Expression that selects the card ID field.</param>
    /// <param name="cardTitleSelector">Expression that selects the card title field.</param>
    /// <param name="cardDescriptionSelector">Expression that selects the card description field.</param>
    /// <returns>A kanban builder for fluent configuration of columns, cards, and formatting.</returns>
    public static KanbanBuilder<TModel, TGroupKey> ToKanban<TModel, TGroupKey>(
        this IEnumerable<TModel> records,
        Expression<Func<TModel, TGroupKey>> groupBySelector,
        Expression<Func<TModel, object?>> cardIdSelector,
        Expression<Func<TModel, object?>> cardTitleSelector,
        Expression<Func<TModel, object?>> cardDescriptionSelector)
        where TGroupKey : notnull
    {
        return new KanbanBuilder<TModel, TGroupKey>(
            records,
            groupBySelector.Compile(),
            cardIdSelector.Compile(),
            cardTitleSelector.Compile(),
            cardDescriptionSelector.Compile());
    }
}
