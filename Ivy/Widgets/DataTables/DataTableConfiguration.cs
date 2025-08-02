// ReSharper disable once CheckNamespace
namespace Ivy;

public class DataTableConfiguration
{
    public bool AllowSearch { get; set; } = true;
    public FilterTypes FilterType { get; set; } = FilterTypes.List;
    public int? FreezeColumns { get; set; } = null;
    public bool AllowSorting { get; set; } = true;
    public bool AllowFiltering { get; set; } = true;
    public bool AllowColumnReordering { get; set; } = true;
    public bool AllowColumnResizing { get; set; } = true;
    public bool AllowCopySelection { get; set; } = true;
    public SelectionModes SelectionMode { get; set; } = SelectionModes.Cells;
    public bool ShowIndexColumn { get; set; } = false;
    public bool ShowGroups { get; set; } = false;
}

public enum FilterTypes
{
    None,
    List,
    Tree
}

public enum SelectionModes
{
    None,
    SingleRow,
    SingleColumn,
    MultipleRows,
    MultipleColumns,
    Cells
}