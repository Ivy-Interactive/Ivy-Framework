using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class DataTableColumn
{
    public required string Name { get; set; }
    
    public required string Header { get; set; } 
    
    public Size? Width { get; set; }
    
    public bool Hidden { get; set; } = false;
    
    public bool Sortable { get; set; } = true;
    
    public Align Align { get; set; } = Align.Left;
    
    public int Order { get; set; } = 0;
}