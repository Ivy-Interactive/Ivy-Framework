using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.DataTables;

public class DataTableView<TModel>(IQueryable<TModel> queryable, Size? width, Size? height, DataTableColumn[] columns) : ViewBase, IStateless
{
    public override object? Build()
    {
        var connection = this.UseDataTable(queryable);
        if (connection == null) return null;
        return new DataTable(connection, width, height, columns);
    }
}