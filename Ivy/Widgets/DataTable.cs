using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record DataTable : WidgetBase<DataTable>
{
    [Prop] public string? ServerUrl { get; set; }
    
    [Prop] public string? Query { get; set; }
    
    [Prop] public int Limit { get; set; } = 100;
    
    [Prop] public int Offset { get; set; } = 0;
    
    [Prop] public bool ShowQueryInput { get; set; } = true;
    
    [Prop] public bool ShowRefreshButton { get; set; } = true;
    
    [Prop] public bool ShowStatus { get; set; } = true;
    
    [Prop] public string? Title { get; set; }
    
    [Prop] public string? Description { get; set; }
}

public static class DataTableExtensions
{
    public static DataTable ServerUrl(this DataTable dataTable, string serverUrl)
    {
        return dataTable with { ServerUrl = serverUrl };
    }
    
    public static DataTable Query(this DataTable dataTable, string query)
    {
        return dataTable with { Query = query };
    }
    
    public static DataTable Limit(this DataTable dataTable, int limit)
    {
        return dataTable with { Limit = limit };
    }
    
    public static DataTable Offset(this DataTable dataTable, int offset)
    {
        return dataTable with { Offset = offset };
    }
    
    public static DataTable ShowQueryInput(this DataTable dataTable, bool show = true)
    {
        return dataTable with { ShowQueryInput = show };
    }
    
    public static DataTable ShowRefreshButton(this DataTable dataTable, bool show = true)
    {
        return dataTable with { ShowRefreshButton = show };
    }
    
    public static DataTable ShowStatus(this DataTable dataTable, bool show = true)
    {
        return dataTable with { ShowStatus = show };
    }
    
    public static DataTable Title(this DataTable dataTable, string title)
    {
        return dataTable with { Title = title };
    }
    
    public static DataTable Description(this DataTable dataTable, string description)
    {
        return dataTable with { Description = description };
    }
} 