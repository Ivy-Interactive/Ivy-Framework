using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class CellClickEventArgs
{
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }
    public string ColumnName { get; set; } = "";
    public object? CellValue { get; set; }
}

public class RowActionClickEventArgs
{
    /// <summary> Id of the row where the event was fired. </summary>
    public object? Id { get; set; }
    /// <summary> Tag of the menu item that was clicked. </summary>
    public object? Tag { get; set; }
}

/// <summary>
/// A comprehensive grid for displaying and interacting with rows of data, supporting sorting and pagination.
/// </summary>
public record DataTable : WidgetBase<DataTable>
{
    public DataTable(
        DataTableConnection connection,
        Size? width,
        Size? height,
        DataTableColumn[] columns,
        DataTableConfig config
    )
    {
        Width = width ?? Size.Full();
        Height = height ?? Size.Full();
        Connection = connection;
        Columns = columns;
        Config = config;
    }

    internal DataTable()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public DataTableColumn[] Columns { get; set; } = [];

    [Prop] public DataTableConnection? Connection { get; set; }

    [Prop] public DataTableConfig? Config { get; set; }

    [Prop] public MenuItem[]? RowActions { get; set; }

    [Event] public EventHandler<Event<DataTable, CellClickEventArgs>>? OnCellClick { get; set; }

    [Event] public EventHandler<Event<DataTable, CellClickEventArgs>>? OnCellActivated { get; set; }

    [Event] public EventHandler<Event<DataTable, RowActionClickEventArgs>>? OnRowAction { get; set; }

    public static Detail operator |(DataTable widget, object child)
    {
        throw new NotSupportedException("DataTable does not support children.");
    }
}

public static class DataTableWidgetExtensions
{
    public static DataTable OnRowAction(this DataTable table, Func<Event<DataTable, RowActionClickEventArgs>, ValueTask> handler)
        => table with { OnRowAction = new(handler) };

    public static DataTable OnCellClick(this DataTable table, Func<Event<DataTable, CellClickEventArgs>, ValueTask> handler)
        => table with { OnCellClick = new(handler) };

    public static DataTable OnCellActivated(this DataTable table, Func<Event<DataTable, CellClickEventArgs>, ValueTask> handler)
        => table with { OnCellActivated = new(handler) };
}