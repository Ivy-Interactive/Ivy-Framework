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

    [Prop] public bool ResizableColumns { get; set; } = false;

    // gRPC-specific properties
    [Prop] public bool UseGrpc { get; set; } = false;

    [Prop] public string[]? SelectColumns { get; set; }

    [Prop] public SortOrder[]? SortOrders { get; set; }

    [Prop] public Filter? Filter { get; set; }

    [Prop] public Aggregation[]? Aggregations { get; set; }
}

public record SortOrder
{
    public string Column { get; set; } = string.Empty;
    public SortDirection Direction { get; set; } = SortDirection.Asc;
}

public enum SortDirection
{
    Asc,
    Desc
}

public record Filter
{
    public Condition? Condition { get; set; }
    public FilterGroup? Group { get; set; }
    public bool Negate { get; set; }
}

public record Condition
{
    public string Column { get; set; } = string.Empty;
    public string Function { get; set; } = string.Empty; // e.g. "equals", "greaterThan", "inSet", "contains"
    public object[] Args { get; set; } = Array.Empty<object>();
}

public record FilterGroup
{
    public LogicalOperator Op { get; set; } = LogicalOperator.And;
    public Filter[] Filters { get; set; } = Array.Empty<Filter>();
}

public enum LogicalOperator
{
    And,
    Or
}

public record Aggregation
{
    public string Column { get; set; } = string.Empty;
    public string Function { get; set; } = string.Empty; // e.g. "sum", "avg", "min", "max", "count"
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

    public static DataTable ResizableColumns(this DataTable dataTable, bool resizable = true)
    {
        return dataTable with { ResizableColumns = resizable };
    }

    // gRPC-specific extensions
    public static DataTable UseGrpc(this DataTable dataTable, bool useGrpc = true)
    {
        return dataTable with { UseGrpc = useGrpc };
    }

    public static DataTable SelectColumns(this DataTable dataTable, params string[] columns)
    {
        return dataTable with { SelectColumns = columns };
    }

    public static DataTable SortBy(this DataTable dataTable, string column, SortDirection direction = SortDirection.Asc)
    {
        var newSort = new SortOrder { Column = column, Direction = direction };
        var existingSorts = dataTable.SortOrders ?? Array.Empty<SortOrder>();
        return dataTable with { SortOrders = existingSorts.Append(newSort).ToArray() };
    }

    public static DataTable Where(this DataTable dataTable, string column, string function, params object[] args)
    {
        var condition = new Condition { Column = column, Function = function, Args = args };
        var filter = new Filter { Condition = condition };
        return dataTable with { Filter = filter };
    }

    public static DataTable WhereAnd(this DataTable dataTable, params Filter[] filters)
    {
        var group = new FilterGroup { Op = LogicalOperator.And, Filters = filters };
        var filter = new Filter { Group = group };
        return dataTable with { Filter = filter };
    }

    public static DataTable WhereOr(this DataTable dataTable, params Filter[] filters)
    {
        var group = new FilterGroup { Op = LogicalOperator.Or, Filters = filters };
        var filter = new Filter { Group = group };
        return dataTable with { Filter = filter };
    }

    public static DataTable Aggregate(this DataTable dataTable, string column, string function)
    {
        var aggregation = new Aggregation { Column = column, Function = function };
        var existingAggs = dataTable.Aggregations ?? Array.Empty<Aggregation>();
        return dataTable with { Aggregations = existingAggs.Append(aggregation).ToArray() };
    }

    // Helper methods for common filters
    public static Filter Equals(string column, object value) => new Filter
    {
        Condition = new Condition { Column = column, Function = "equals", Args = new[] { value } }
    };

    public static Filter GreaterThan(string column, object value) => new Filter
    {
        Condition = new Condition { Column = column, Function = "greaterThan", Args = new[] { value } }
    };

    public static Filter LessThan(string column, object value) => new Filter
    {
        Condition = new Condition { Column = column, Function = "lessThan", Args = new[] { value } }
    };

    public static Filter InSet(string column, params object[] values) => new Filter
    {
        Condition = new Condition { Column = column, Function = "inSet", Args = values }
    };

    public static Filter Contains(string column, string value) => new Filter
    {
        Condition = new Condition { Column = column, Function = "contains", Args = new[] { value } }
    };
}