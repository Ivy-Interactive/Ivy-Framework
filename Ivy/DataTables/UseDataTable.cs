using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.DataTables;

public static class UseDataTableExtensions
{
    public static IState<string?> UseDataTable<TView>(this TView view, IQueryable queryable) where TView : ViewBase =>
        view.Context.UseDataTable(queryable);
    
    public static IState<string?> UseDataTable(this IViewContext context, IQueryable queryable)
    {
        var url = context.UseState<string?>();
        var dataTableService = context.UseService<IDataTableService>();
        context.UseEffect(() =>
        {
            var (cleanup, queryUrl) = dataTableService.AddQueryable(queryable);
            url.Set(queryUrl);
            return cleanup;
        });
        return url;
    }
}