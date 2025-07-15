using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.DataTables;

public class DataTableView<TModel>(IQueryable<TModel> queryable, Size? width, Size? height, DataTableColumn[] columns) : ViewBase, IStateless
{
    public override object? Build()
    {
        var queryUrl = this.UseDataTable(queryable);
        return new DataTable(queryUrl.Value!, width, height, columns);
    }
}