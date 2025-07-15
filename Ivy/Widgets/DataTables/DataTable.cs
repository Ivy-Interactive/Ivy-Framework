using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record DataTable : WidgetBase<DataTable>
{
    public DataTable(string queryUrl, Size? width, Size? height, DataTableColumn[] columns) : base()
    {
        Width = width;
        Height = height;
        QueryUrl = queryUrl;
        Columns = columns;
    }
    
    [Prop] public Size? Width { get; set; }
    
    [Prop] public Size? Height { get; set; }
    
    [Prop] public string? QueryUrl { get; set; }
    
    [Prop] public DataTableColumn[]? Columns { get; set; }
    
    public static Detail operator |(DataTable widget, object child)
    {
        throw new NotSupportedException("DataTable does not support children.");
    }
}