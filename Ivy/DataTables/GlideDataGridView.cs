using Ivy.Core;
using Ivy.Shared;

namespace Ivy.DataTables;

public class GlideDataGridView(IQueryable queryable, Size? width, Size? height, DataTableColumn[] columns, DataTableConfiguration configuration) : ViewBase
{
    public override object? Build()
    {
        var connection = this.UseDataTable(queryable);
        if (connection == null) return null;
        return new GlideDataGrid(connection, width, height, columns, configuration);
    }
}