using System.Linq.Expressions;

namespace Ivy.Views.DataTables;

public static class DataTableExtensions
{
    public static DataTableBuilder<TModel> ToDataTable<TModel>(this IQueryable<TModel> queryable)
    {
        return new DataTableBuilder<TModel>(queryable);
    }

    public static DataTableBuilder<TModel> ToDataTable<TModel>(
        this IQueryable<TModel> queryable,
        Expression<Func<TModel, object?>> idSelector)
    {
        var builder = new DataTableBuilder<TModel>(queryable, idSelector);
        builder.Initialize();
        return builder;
    }
}
