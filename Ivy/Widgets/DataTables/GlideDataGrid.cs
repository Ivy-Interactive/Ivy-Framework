using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record GlideDataGrid : WidgetBase<GlideDataGrid>
{
    public GlideDataGrid(
        DataTableConnection connection, 
        Size? width, 
        Size? height, 
        DataTableColumn[] columns,
        DataTableConfiguration configuration
    )
    {
        Width = width;
        Height = height;
        Connection = connection;
        Columns = columns;
        Configuration = configuration;
    }
    
    [Prop] public DataTableColumn[] Columns { get; set; }
    
    [Prop] public DataTableConnection Connection { get; set; }

    [Prop] public DataTableConfiguration Configuration { get; set; }

    public static Detail operator |(GlideDataGrid widget, object child)
    {
        throw new NotSupportedException("GlideDataGrid does not support children.");
    }
}