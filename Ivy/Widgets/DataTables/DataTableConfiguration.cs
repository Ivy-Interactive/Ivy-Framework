// ReSharper disable once CheckNamespace
namespace Ivy;

public record DataTableConfiguration
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
    public int? BatchSize { get; set; } = null;
    public bool LoadAllRows { get; set; } = false;
}

public enum SelectionModes
{
    None,
    Rows,
    Columns,
    Cells
}