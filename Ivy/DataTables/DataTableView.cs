using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.DataTables;

public class DataTableView(IQueryable queryable, Size? width, Size? height, DataTableColumn[] columns, DataTableConfiguration configuration) : ViewBase, IStateless
{
    public override object? Build()
    {
        var connection = this.UseDataTable(queryable);
        if (connection == null) return null;
        return new DataTable(connection, width, height, columns, configuration);
    }
}