using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Views.DataTables;

public class DataTableView(
    IQueryable queryable,
    Size? width,
    Size? height,
    DataTableColumn[] columns,
    DataTableConfiguration configuration,
    Func<Event<DataTable, (int RowIndex, int ColumnIndex, string ColumnName, object? CellValue)>, ValueTask>? onCellClick = null,
    Func<Event<DataTable, (int RowIndex, int ColumnIndex, string ColumnName, object? CellValue)>, ValueTask>? onCellActivated = null) : ViewBase
{
    public override object? Build()
    {
        var connection = this.UseDataTable(queryable);
        if (connection == null) return null;

        // Store handlers in state so they persist across rebuilds
        var handlers = this.UseState(() => (onCellClick, onCellActivated));

        var dataTable = new DataTable(connection, width, height, columns, configuration);

        // Attach OnCellClick handler if specified
        if (handlers.Value.onCellClick != null)
        {
            dataTable = dataTable with { OnCellClick = handlers.Value.onCellClick };
        }

        // Attach OnCellActivated handler if specified
        if (handlers.Value.onCellActivated != null)
        {
            dataTable = dataTable with { OnCellActivated = handlers.Value.onCellActivated };
        }

        return dataTable;
    }
}