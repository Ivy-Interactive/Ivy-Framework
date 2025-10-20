namespace Ivy.Views.DataTables;

public static class DataTableExtensions
{
    public static DataTableBuilder<TModel> ToDataTable<TModel>(this IQueryable<TModel> queryable)
    {
        return new DataTableBuilder<TModel>(queryable);
    }
}
