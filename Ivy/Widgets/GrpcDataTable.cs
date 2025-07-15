using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record GrpcDataTable : DataTable
{
    public GrpcDataTable() : base()
    {
        UseGrpc = true;
    }
}

public static class GrpcDataTableExtensions
{
    public static GrpcDataTable ServerUrl(this GrpcDataTable dataTable, string serverUrl)
    {
        return dataTable with { ServerUrl = serverUrl };
    }

    public static GrpcDataTable SelectColumns(this GrpcDataTable dataTable, params string[] columns)
    {
        return dataTable with { SelectColumns = columns };
    }

    public static GrpcDataTable SortBy(this GrpcDataTable dataTable, string column, SortDirection direction = SortDirection.Asc)
    {
        var newSort = new SortOrder { Column = column, Direction = direction };
        var existingSorts = dataTable.SortOrders ?? Array.Empty<SortOrder>();
        return dataTable with { SortOrders = existingSorts.Append(newSort).ToArray() };
    }

    public static GrpcDataTable Where(this GrpcDataTable dataTable, string column, string function, params object[] args)
    {
        var condition = new Condition { Column = column, Function = function, Args = args };
        var filter = new Filter { Condition = condition };
        return dataTable with { Filter = filter };
    }

    public static GrpcDataTable WhereAnd(this GrpcDataTable dataTable, params Filter[] filters)
    {
        var group = new FilterGroup { Op = LogicalOperator.And, Filters = filters };
        var filter = new Filter { Group = group };
        return dataTable with { Filter = filter };
    }

    public static GrpcDataTable WhereOr(this GrpcDataTable dataTable, params Filter[] filters)
    {
        var group = new FilterGroup { Op = LogicalOperator.Or, Filters = filters };
        var filter = new Filter { Group = group };
        return dataTable with { Filter = filter };
    }

    public static GrpcDataTable Aggregate(this GrpcDataTable dataTable, string column, string function)
    {
        var aggregation = new Aggregation { Column = column, Function = function };
        var existingAggs = dataTable.Aggregations ?? Array.Empty<Aggregation>();
        return dataTable with { Aggregations = existingAggs.Append(aggregation).ToArray() };
    }

    public static GrpcDataTable Limit(this GrpcDataTable dataTable, int limit)
    {
        return dataTable with { Limit = limit };
    }

    public static GrpcDataTable Offset(this GrpcDataTable dataTable, int offset)
    {
        return dataTable with { Offset = offset };
    }

    public static GrpcDataTable ShowQueryInput(this GrpcDataTable dataTable, bool show = true)
    {
        return dataTable with { ShowQueryInput = show };
    }

    public static GrpcDataTable ShowRefreshButton(this GrpcDataTable dataTable, bool show = true)
    {
        return dataTable with { ShowRefreshButton = show };
    }

    public static GrpcDataTable ShowStatus(this GrpcDataTable dataTable, bool show = true)
    {
        return dataTable with { ShowStatus = show };
    }

    public static GrpcDataTable Title(this GrpcDataTable dataTable, string title)
    {
        return dataTable with { Title = title };
    }

    public static GrpcDataTable Description(this GrpcDataTable dataTable, string description)
    {
        return dataTable with { Description = description };
    }

    public static GrpcDataTable ResizableColumns(this GrpcDataTable dataTable, bool resizable = true)
    {
        return dataTable with { ResizableColumns = resizable };
    }
}