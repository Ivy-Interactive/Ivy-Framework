using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Event arguments for cell click events in a DataTable.</summary>
public class CellClickEventArgs
{
    /// <summary>The row index of the clicked cell.</summary>
    public int RowIndex { get; set; }

    /// <summary>The column index of the clicked cell.</summary>
    public int ColumnIndex { get; set; }

    /// <summary>The name of the column for the clicked cell.</summary>
    public string ColumnName { get; set; } = "";

    /// <summary>The value of the clicked cell.</summary>
    public object? CellValue { get; set; }
}

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

    [Prop] public DataTableColumn[] Columns { get; set; }

    [Prop] public DataTableConnection Connection { get; set; }

    [Prop] public DataTableConfig Config { get; set; }

    /// <summary>Event handler called when a cell is clicked (single-click).</summary>
    [Event] public Func<Event<DataTable, CellClickEventArgs>, ValueTask>? OnCellClick { get; set; }

    /// <summary>Event handler called when a cell is activated (double-clicked for editing).</summary>
    [Event] public Func<Event<DataTable, CellClickEventArgs>, ValueTask>? OnCellActivated { get; set; }

    public static Detail operator |(DataTable widget, object child)
    {
        throw new NotSupportedException("DataTable does not support children.");
    }
}