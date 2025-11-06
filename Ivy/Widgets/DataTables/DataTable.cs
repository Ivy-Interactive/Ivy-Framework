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

    public static Detail operator |(DataTable widget, object child)
    {
        throw new NotSupportedException("DataTable does not support children.");
    }
}