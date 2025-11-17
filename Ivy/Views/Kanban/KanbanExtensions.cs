using System.Linq.Expressions;

namespace Ivy.Views.Kanban;

public static class KanbanExtensions
{
    public static KanbanBuilder<TModel, TGroupKey> ToKanban<TModel, TGroupKey>(
        this IEnumerable<TModel> records,
        Expression<Func<TModel, TGroupKey>> groupBySelector)
        where TGroupKey : notnull
    {
        return new KanbanBuilder<TModel, TGroupKey>(records, groupBySelector.Compile());
    }

    public static KanbanBuilder<TModel, TGroupKey> ToKanban<TModel, TGroupKey>(
        this IEnumerable<TModel> records,
        Expression<Func<TModel, TGroupKey>> groupBySelector,
        Expression<Func<TModel, object?>> idSelector,
        Expression<Func<TModel, object?>> titleSelector,
        Expression<Func<TModel, object?>> descriptionSelector)
        where TGroupKey : notnull
    {
        return new KanbanBuilder<TModel, TGroupKey>(
            records,
            groupBySelector.Compile(),
            idSelector.Compile(),
            titleSelector.Compile(),
            descriptionSelector.Compile(),
            null);
    }

    public static KanbanBuilder<TModel, TGroupKey> ToKanban<TModel, TGroupKey>(
        this IEnumerable<TModel> records,
        Expression<Func<TModel, TGroupKey>> groupBySelector,
        Expression<Func<TModel, object?>> idSelector,
        Expression<Func<TModel, object?>> titleSelector,
        Expression<Func<TModel, object?>> descriptionSelector,
        Expression<Func<TModel, object?>> orderSelector)
        where TGroupKey : notnull
    {
        return new KanbanBuilder<TModel, TGroupKey>(
            records,
            groupBySelector.Compile(),
            idSelector.Compile(),
            titleSelector.Compile(),
            descriptionSelector.Compile(),
            orderSelector.Compile());
    }
}
