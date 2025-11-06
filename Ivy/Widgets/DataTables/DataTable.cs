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

/// <summary>Configuration for a row action button.</summary>
public class RowAction
{
    /// <summary>Unique identifier for this action.</summary>
    public string Id { get; set; } = "";

    /// <summary>Icon name (Lucide icon).</summary>
    public string Icon { get; set; } = "";

    /// <summary>Event name to trigger when clicked (e.g., "OnEdit", "OnDelete", "OnView").</summary>
    public string EventName { get; set; } = "";

    /// <summary>Tooltip text for the button.</summary>
    public string? Tooltip { get; set; }
}

/// <summary>Event arguments for row action click events.</summary>
public class RowActionClickEventArgs
{
    /// <summary>The ID of the action that was clicked.</summary>
    public string ActionId { get; set; } = "";

    /// <summary>The event name of the action.</summary>
    public string EventName { get; set; } = "";

    /// <summary>The index of the clicked row.</summary>
    public int RowIndex { get; set; }

    /// <summary>The data for the clicked row, keyed by column name.</summary>
    public Dictionary<string, object?> RowData { get; set; } = new();
}

public record DataTable : WidgetBase<DataTable>
{
    public DataTable(
        DataTableConnection connection,
        Size? width,
        Size? height,
        DataTableColumn[] columns,
        DataTableConfiguration configuration
    )
    {
        Width = width ?? Size.Full();
        Height = height ?? Size.Full();
        Connection = connection;
        Columns = columns;
        Configuration = configuration;
    }

    [Prop] public DataTableColumn[] Columns { get; set; }

    [Prop] public DataTableConnection Connection { get; set; }

    [Prop] public DataTableConfiguration Configuration { get; set; }

    [Prop] public RowAction[]? RowActions { get; set; }

    /// <summary>
    /// Event handler called when a cell is clicked (single-click).
    /// Use this for quick actions like showing previews, navigation, or selection.
    /// </summary>
    /// <remarks>
    /// <para><strong>Best Practice:</strong> Avoid using both OnCellClick and OnCellActivated simultaneously as this can create UX conflicts.</para>
    /// <para><strong>When to use:</strong> Quick actions, navigation, showing details in a side panel, row selection.</para>
    /// <para><strong>Note:</strong> OnCellClick fires before OnCellActivated on double-click, which may cause unexpected behavior.</para>
    /// </remarks>
    [Event] public Func<Event<DataTable, CellClickEventArgs>, ValueTask>? OnCellClick { get; set; }

    /// <summary>
    /// Event handler called when a cell is activated (double-clicked).
    /// Use this for deliberate actions like opening edit dialogs or drilling into details.
    /// </summary>
    /// <remarks>
    /// <para><strong>Best Practice:</strong> Avoid using both OnCellClick and OnCellActivated simultaneously as this can create UX conflicts.</para>
    /// <para><strong>When to use:</strong> Opening edit dialogs/sheets, entering edit mode, drilling down into details.</para>
    /// <para><strong>Mobile Note:</strong> Double-click is awkward on touch devices. Consider mobile users when choosing this event.</para>
    /// </remarks>
    [Event] public Func<Event<DataTable, CellClickEventArgs>, ValueTask>? OnCellActivated { get; set; }

    /// <summary>
    /// Event handler called when a row action button is clicked.
    /// This single event handles all row action clicks, with the EventName indicating which action was triggered.
    /// </summary>
    /// <remarks>
    /// <para><strong>When to use:</strong> Row-level actions like Edit, Delete, View, Download, etc.</para>
    /// <para><strong>Note:</strong> The EventName property in the event args indicates which action was clicked.</para>
    /// <para><strong>Example:</strong> Use switch on EventName to handle different actions (e.g., "OnEdit", "OnDelete", "OnView").</para>
    /// </remarks>
    [Event] public Func<Event<DataTable, RowActionClickEventArgs>, ValueTask>? OnRowAction { get; set; }

    public static Detail operator |(DataTable widget, object child)
    {
        throw new NotSupportedException("DataTable does not support children.");
    }
}