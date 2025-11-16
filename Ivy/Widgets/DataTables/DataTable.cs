using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Event arguments for cell click events.</summary>
public class CellClickEventArgs
{
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }
    public string ColumnName { get; set; } = "";
    public object? CellValue { get; set; }
}

/// <summary>Event arguments for row action click events.</summary>
public class RowActionClickEventArgs
{
    public string ActionId { get; set; } = "";
    public string EventName { get; set; } = "";
    public int RowIndex { get; set; }
    public Dictionary<string, object?> RowData { get; set; } = new();
}

/// <summary>Configuration for a row action button.</summary>
public record RowAction
{
    public string Id { get; set; } = "";
    public string Icon { get; set; } = "";
    public string EventName { get; set; } = "";
    public string? Tooltip { get; set; }
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

    [Prop] public RowAction[]? RowActions { get; set; }

    /// <summary>Called when a cell is clicked (single-click).</summary>
    [Event] public Func<Event<DataTable, CellClickEventArgs>, ValueTask>? OnCellClick { get; set; }

    /// <summary>Called when a cell is activated (double-clicked).</summary>
    [Event] public Func<Event<DataTable, CellClickEventArgs>, ValueTask>? OnCellActivated { get; set; }

    /// <summary>Called when a row action button is clicked.</summary>
    [Event] public Func<Event<DataTable, RowActionClickEventArgs>, ValueTask>? OnRowAction { get; set; }

    public static Detail operator |(DataTable widget, object child)
    {
        throw new NotSupportedException("DataTable does not support children.");
    }
}