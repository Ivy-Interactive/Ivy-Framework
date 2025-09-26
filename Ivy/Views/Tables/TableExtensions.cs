namespace Ivy.Views.Tables;

/// <summary>
/// Extension methods for converting collections to table builders with fluent configuration.
/// </summary>
public static class TableExtensions
{
    /// <summary>
    /// Converts any enumerable collection to a table builder with automatic column scaffolding.
    /// </summary>
    /// <typeparam name="TModel">The type of objects in the collection.</typeparam>
    /// <param name="records">The collection to convert to a table.</param>
    /// <returns>A table builder for fluent configuration of columns, headers, and formatting.</returns>
    public static TableBuilder<TModel> ToTable<TModel>(this IEnumerable<TModel> records)
    {
        return new TableBuilder<TModel>(records);
    }
}
