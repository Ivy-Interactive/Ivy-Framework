using Ivy.Core;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Ivy;

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
    Func<object, object?>? idSelector = null,
    RefreshToken? refreshToken = null) : ViewBase, IMemoized
{
    public override object? Build()
    {
        var connection = UseDataTable(queryable, idSelector, refreshToken);
        if (connection == null)
        {
            return null;
        }

        var table = new DataTable(connection, width, height, columns, config)
        {
            OnCellClick = onCellClick.ToEventHandler(),
            OnCellActivated = onCellActivated.ToEventHandler(),
            RowActions = rowActions,
            OnRowAction = onRowAction.ToEventHandler()
        };

        return table;
    }

    public object[] GetMemoValues()
    {
        // Memoize based on queryable and configuration
        // Don't include the queryable itself as it might change reference
        // Only memoize if all inputs are stable
        return [(object?)width!, (object?)height!, columns, config, refreshToken?.Token!];
    }
}
