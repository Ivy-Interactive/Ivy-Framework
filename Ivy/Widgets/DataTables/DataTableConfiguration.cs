using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class DataTableConfiguration
{
    public int? FreezeColumns { get; set; } = null;
    public bool AllowSorting { get; set; } = true;
    public bool AllowFiltering { get; set; } = true;
    public bool AllowColumnReordering { get; set; } = true;
    public bool AllowColumnResizing { get; set; } = true;
    public bool AllowCopy { get; set; } = true;
    public bool AllowCellSelection { get; set; } = true;
    public bool AllowRowSelection { get; set; } = false;
    public bool AllowColumnSelection { get; set; } = false;
}