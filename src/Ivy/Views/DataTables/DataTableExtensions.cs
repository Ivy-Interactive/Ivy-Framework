using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class DataTableExtensions
{
    public static DataTableBuilder<TModel> ToDataTable<TModel>(this IQueryable<TModel> queryable)
    {
        var builder = new DataTableBuilder<TModel>(queryable);
        SetDefaultEmptyState(builder);
        return builder;
    }

    public static DataTableBuilder<TModel> ToDataTable<TModel>(
        this IQueryable<TModel> queryable,
        Expression<Func<TModel, object?>> idSelector)
    {
        var builder = new DataTableBuilder<TModel>(queryable, idSelector);
        builder.Initialize();
        SetDefaultEmptyState(builder);
        return builder;
    }

    private static void SetDefaultEmptyState<TModel>(DataTableBuilder<TModel> builder)
    {
        builder.Empty((context) => new Stack()
            .Padding(Spacing.ExtraLarge)
            .Gap(Spacing.Medium)
            .AlignItems(Align.Center)
            | new Text("No items found")
                .Size(TextSize.Large)
                .Color(Colors.Muted)
            | new Text("This table is currently empty.")
                .Color(Colors.Muted)
        );
    }
}
