using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Views.DataTables;

public class DataTableView(
    IQueryable queryable,
    Size? width,
    Size? height,
    DataTableColumn[] columns,
    DataTableConfiguration configuration,
    Func<Event<DataTable, CellClickEventArgs>, ValueTask>? onCellClick = null,
    Func<Event<DataTable, CellClickEventArgs>, ValueTask>? onCellActivated = null) : ViewBase
{
    public override object? Build()
    {
        var connection = this.UseDataTable(queryable);
        if (connection == null) return null;

        return new DataTable(connection, width, height, columns, configuration)
        {
            OnCellClick = onCellClick,
            OnCellActivated = onCellActivated
        };
    }
}