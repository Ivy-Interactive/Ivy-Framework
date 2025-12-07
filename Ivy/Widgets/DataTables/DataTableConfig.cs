// ReSharper disable once CheckNamespace
namespace Ivy;

public record DataTableConfig
{
    public int? FreezeColumns { get; set; } = null;
    public bool AllowSorting { get; set; } = true;
    public bool AllowFiltering { get; set; } = true;
    public bool AllowLlmFiltering { get; set; } = false;
    public bool AllowColumnReordering { get; set; } = true;
    public bool AllowColumnResizing { get; set; } = true;
    public bool AllowCopySelection { get; set; } = true;
    public SelectionModes SelectionMode { get; set; } = SelectionModes.Cells;
    public bool ShowIndexColumn { get; set; } = false;
    public bool ShowGroups { get; set; } = false;
    public bool ShowColumnTypeIcons { get; set; } = false;
    public bool ShowVerticalBorders { get; set; } = true;
    public int? BatchSize { get; set; } = null;
    public bool LoadAllRows { get; set; } = false;
    public bool EnableCellClickEvents { get; set; } = false;
    public bool ShowSearch { get; set; } = false;
    public string? IdColumnName { get; set; } = null;
}

public enum SelectionModes
{
    None,
    Rows,
    Columns,
    Cells
}