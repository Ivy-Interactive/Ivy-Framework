// ReSharper disable once CheckNamespace
namespace Ivy;

public static partial class TableExtensions
{
    public static TableBuilder<TModel> ToTable<TModel>(this IEnumerable<TModel> records)
    {
        return new TableBuilder<TModel>(records);
    }
}
