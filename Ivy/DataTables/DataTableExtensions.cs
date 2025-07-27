namespace Ivy.DataTables;

public static class TableExtensions
{
    public static DataTableBuilder<TModel> ToDataTable<TModel>(this IQueryable<TModel> queryable)
    {
        return new DataTableBuilder<TModel>(queryable);
    }
    
    public static GlideDataGridBuilder<TModel> ToGlideDataGrid<TModel>(this IQueryable<TModel> queryable)
    {
        return new GlideDataGridBuilder<TModel>(queryable);
    }
}
