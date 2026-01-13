namespace Ivy.Views.Tables;

public static class TableExtensions
{
    public static TableBuilder<TModel> ToTable<TModel>(this IEnumerable<TModel> records)
    {
        return new TableBuilder<TModel>(records);
    }
}
