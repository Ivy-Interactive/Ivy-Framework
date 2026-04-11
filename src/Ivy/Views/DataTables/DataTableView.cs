using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class DataTableView(
    IQueryable queryable,
    Size? width,
    Size? height,
    DataTableColumn[] columns,
    DataTableConfig config,
    Density density = Density.Medium,
    Func<Event<DataTable, CellClickEventArgs>, ValueTask>? onCellClick = null,
    Func<Event<DataTable, CellClickEventArgs>, ValueTask>? onCellActivated = null,
    MenuItem[]? rowActions = null,
    Func<Event<DataTable, RowActionClickEventArgs>, ValueTask>? onRowAction = null,
    Func<object, object?>? idSelector = null,
    RefreshToken? refreshToken = null,
    FuncViewBuilder? emptyViewFactory = null,
    FuncViewBuilder? headerLeftFactory = null,
    FuncViewBuilder? headerRightFactory = null) : ViewBase, IMemoized
{
    public override object? Build()
    {
        var connection = UseDataTable(queryable, idSelector, columns, refreshToken, config);
        if (connection == null)
        {
            return null;
        }

        var table = new DataTable(connection, width, height, columns, config)
        {
            Density = density,
            OnCellClick = onCellClick.ToEventHandler(),
            OnCellActivated = onCellActivated.ToEventHandler(),
            RowActions = rowActions,
            OnRowAction = onRowAction.ToEventHandler()
        };

        var slots = new List<Slot>();

        if (emptyViewFactory != null)
            slots.Add(new Slot("EmptyView", emptyViewFactory(Context)));
        if (headerLeftFactory != null)
            slots.Add(new Slot("HeaderLeft", headerLeftFactory(Context)));
        if (headerRightFactory != null)
            slots.Add(new Slot("HeaderRight", headerRightFactory(Context)));

        if (slots.Count > 0)
            table = table with { Children = slots.ToArray<object>() };

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
