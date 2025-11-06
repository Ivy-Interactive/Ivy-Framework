using Ivy.Core;
using Ivy.Shared;
using Microsoft.Extensions.Logging;

namespace Ivy.Views.DataTables;

public class DataTableView(
    IQueryable queryable,
    Size? width,
    Size? height,
    DataTableColumn[] columns,
    DataTableConfiguration configuration,
    Func<Event<DataTable, CellClickEventArgs>, ValueTask>? onCellClick = null,
    Func<Event<DataTable, CellClickEventArgs>, ValueTask>? onCellActivated = null) : ViewBase, IMemoized
{
    public override object? Build()
    {
        var connection = this.UseDataTable(queryable);
        if (connection == null)
        {
            return null;
        }

        var table = new DataTable(connection, width, height, columns, configuration)
        {
            OnCellClick = onCellClick,
            OnCellActivated = onCellActivated
        };

        return table;
    }

    public object[] GetMemoValues()
    {
        // Memoize based on queryable and configuration
        // Don't include the queryable itself as it might change reference
        // Only memoize if all inputs are stable
        return [(object?)width!, (object?)height!, columns, configuration];
    }
}