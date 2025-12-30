using Ivy.Core;
using Ivy.Shared;
using Microsoft.Extensions.Logging;

namespace Ivy.Views.DataTables;

public class DataTableView(
    IQueryable queryable,
    Size? width,
    Size? height,
    DataTableColumn[] columns,
    DataTableConfig config,
    Func<Event<DataTable, CellClickEventArgs>, ValueTask>? onCellClick = null,
    Func<Event<DataTable, CellClickEventArgs>, ValueTask>? onCellActivated = null,
    MenuItem[]? rowActions = null,
    Func<Event<DataTable, RowActionClickEventArgs>, ValueTask>? onRowAction = null,
    Func<object, object?>? idSelector = null) : ViewBase, IMemoized
{
    public override object? Build()
    {
        var connection = this.UseDataTable(queryable, idSelector);
        if (connection == null)
        {
            return null;
        }

        var table = new DataTable(connection, width, height, columns, config)
        {
            OnCellClick = onCellClick,
            OnCellActivated = onCellActivated,
            RowActions = rowActions,
            OnRowAction = onRowAction
        };

        return table;
    }

    public object[] GetMemoValues()
    {
        // Memoize based on queryable and configuration
        // Don't include the queryable itself as it might change reference
        // Only memoize if all inputs are stable
        return [(object?)width!, (object?)height!, columns, config];
    }
}